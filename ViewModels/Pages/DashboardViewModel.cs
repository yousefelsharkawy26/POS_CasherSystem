using POS_ModernUI.Models;
using System.Collections.ObjectModel;
using POS_ModernUI.DataAccess.UnitOfWork;
using POS_ModernUI.Models.DTOs;
using POS_ModernUI.ViewModels.Windows;
using POS_ModernUI.Helpers;
using POS_ModernUI.Services.Contracts;
using System.Threading.Tasks;
using POS_ModernUI.ViewModels.Dialogs;
using POS_ModernUI.Views.Dialogs;

namespace POS_ModernUI.ViewModels.Pages;

public partial class DashboardViewModel : ObservableObject
{
    #region Constants
    private const string PIECE_UNIT = "قطعة";
    private const string BOX_UNIT = "علبة";
    private const string CARTON_UNIT = "كرتونة";
    private const string KILO_UNIT = "كيلو";
    private const string GRAM_UNIT = "جرام";
    private const string POUND_UNIT = "جنيه";
    private const decimal GRAM_TO_KILO_FACTOR = 1000m;
    #endregion

    #region Fields
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDebtServices _debtServices;
    private readonly IDialogService _dialogService;
    private SalesCasherModel? _selectedProduct;
    private string _selectedUnit = PIECE_UNIT;
    private SalesOrder? _currentOrder;
    #endregion

    #region Observable Properties
    [ObservableProperty] private bool _isWithoutBarCode = false;
    [ObservableProperty] private decimal _totalAmount = 0;
    [ObservableProperty] private int _quantity;
    [ObservableProperty] private bool _isPieceEnabled = false;
    [ObservableProperty] private bool _isBoxEnabled = false;
    [ObservableProperty] private bool _isCartonEnabled = false;
    [ObservableProperty] private bool _isUnitsEnabled = false;
    [ObservableProperty] private ObservableCollection<SalesCasherModel> _listOfSales = new();
    [ObservableProperty] private ObservableCollection<Product> _productList = new();

    #endregion

    #region Constructor
    public DashboardViewModel(IUnitOfWork unitOfWork,
                              IDebtServices debtServices,
                              IDialogService dialogService)
    {
        _debtServices = debtServices;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _ = LoadProductList();
        _dialogService = dialogService;
    }
    #endregion

    #region Initialization
    private async Task LoadProductList()
    {
        var products = IsWithoutBarCode
            ? await _unitOfWork.Products.GetAllAsync(u => u.UnitShares.Any(a => string.IsNullOrWhiteSpace(a.ProductBarCode)) && u.QuantityInStock > 0, "UnitShares")
            : await _unitOfWork.Products.GetAllAsync(u => u.QuantityInStock > 0, "UnitShares");

        ProductList = new ObservableCollection<Product>(products.OrderBy(u => u.Name));
    }

    #endregion

    #region Property Change Handlers
    partial void OnQuantityChanged(int value)
    {
        if (_selectedProduct == null || value < 0)
        {
            ResetToCurrentQuantity();
            return;
        }

        if (!IsQuantityValid(value))
        {
            ResetToCurrentQuantity();
            return;
        }

        UpdateProductQuantity(value);
    }
    #endregion

    #region Commands
    [RelayCommand]
    private async Task OnAddProduct(Product product)
    {
        if (product == null) return;

        var defaultProductUnit = await _unitOfWork.ProductUnits
            .GetAsync(u => u.ProductId == product.ProductId, "Unit");

        if (defaultProductUnit == null) return;

        var productUnitNames = (await _unitOfWork.ProductUnits
            .GetAllAsync(u => u.ProductId == product.ProductId, "Unit"))
            .Select(u => u.Unit.Name)
            .ToList();

        SetUnitEnabledState(productUnitNames);
        await AddProduct(product, defaultProductUnit);
    }

    [RelayCommand]
    private async Task OnToggleChecked()
    {
        await LoadProductList();
    }

    [RelayCommand]
    private void OnChangeQuantity(object btnContent)
    {
        if (_selectedProduct == null || !int.TryParse(btnContent?.ToString(), out int digit))
            return;

        int newQuantity = (Quantity * 10) + digit;

        if (_selectedUnit == POUND_UNIT)
        {
            HandlePoundUnitChange(newQuantity);
            return;
        }

        if (!IsQuantityValidForUnit(newQuantity))
            return;

        UpdateProductQuantity(newQuantity);
    }

    [RelayCommand]
    private void OnSelectByPrice()
    {
        if (_selectedProduct == null) return;

        _selectedProduct.TotalPrice = Quantity;
        _selectedProduct.Quantity = CalculateQuantityFromPrice(Quantity, _selectedProduct.UnitPrice);
        _selectedUnit = POUND_UNIT;

        RefreshSalesData();
    }
    [RelayCommand]
    private async Task OnSelectByBox()
    {
        if (_selectedProduct == null || _selectedProduct.UnitName == BOX_UNIT) return;

        _selectedProduct.UnitName = BOX_UNIT;
        _selectedUnit = BOX_UNIT;
        _selectedProduct.UnitPrice = (await _unitOfWork.ProductUnits
            .GetAsync(u => u.ProductId == _selectedProduct.ProductId && u.Unit.Name == BOX_UNIT, "Unit"))?
            .UnitPrice ?? 0;

        RecalculateTotalPrice();
        RefreshSalesData();
    }
    [RelayCommand]
    private async Task OnSelectByCarton()
    {
        if (_selectedProduct == null || _selectedProduct.UnitName == CARTON_UNIT) return;

        _selectedProduct.UnitName = CARTON_UNIT;
        _selectedUnit = CARTON_UNIT;
        _selectedProduct.UnitPrice = (await _unitOfWork.ProductUnits
            .GetAsync(u => u.ProductId == _selectedProduct.ProductId && u.Unit.Name == CARTON_UNIT, "Unit"))?
            .UnitPrice ?? 0;

        RecalculateTotalPrice();
        RefreshSalesData();
    }

    [RelayCommand]
    private void OnSelectByKilo()
    {
        if (_selectedProduct == null || _selectedProduct.UnitName == KILO_UNIT) return;

        _selectedProduct.UnitPrice *= GRAM_TO_KILO_FACTOR;
        _selectedProduct.UnitName = KILO_UNIT;
        _selectedUnit = KILO_UNIT;

        RecalculateTotalPrice();
        RefreshSalesData();
    }

    [RelayCommand]
    private void OnSelectByGram()
    {
        if (_selectedProduct == null || _selectedProduct.UnitName == GRAM_UNIT) return;

        _selectedProduct.UnitPrice /= GRAM_TO_KILO_FACTOR;
        _selectedProduct.UnitName = GRAM_UNIT;
        _selectedUnit = GRAM_UNIT;

        RecalculateTotalPrice();
        RefreshSalesData();
    }

    [RelayCommand]
    private async Task OnSelectByPiece()
    {
        if (_selectedProduct == null || _selectedProduct.UnitName == PIECE_UNIT) return;

        _selectedProduct.UnitName = PIECE_UNIT;
        _selectedUnit = PIECE_UNIT;
        _selectedProduct.UnitPrice = (await _unitOfWork.ProductUnits
            .GetAsync(u => u.ProductId == _selectedProduct.ProductId && u.Unit.Name == PIECE_UNIT, "Unit"))?
            .UnitPrice ?? 0;

        RecalculateTotalPrice();
        RefreshSalesData();
    }

    [RelayCommand]
    private void OnQuantityBack()
    {
        if (_selectedProduct == null)
        {
            Quantity = 0;
            return;
        }

        if (_selectedProduct.Quantity > 0)
        {
            int newQuantity = Quantity / 10;
            UpdateProductQuantity(newQuantity);
        }
        else
        {
            Quantity = 0;
        }
    }
    
    [RelayCommand]
    private void OnClearQuantity()
    {
        if (_selectedProduct == null)
            return;

        UpdateProductQuantity(0);
    }

    [RelayCommand]
    private void OnRemoveProduct(SalesCasherModel salesCasherModel)
    {
        if (salesCasherModel == null) return;

        ListOfSales.Remove(salesCasherModel);
        TotalAmount -= salesCasherModel.TotalPrice;

        if (ListOfSales.Count == 0)
        {
            ResetSelection();
        }
    }

    [RelayCommand]
    private void OnOrderCanceled()
    {
        ClearOrder();
    }

    [RelayCommand]
    private async Task OnOrderCompleted()
    {
        if (ListOfSales.Count == 0) return;

        try
        {
            await ProcessOrderAsync();
            ClearOrder();
            await LoadProductList();
        }
        catch (Exception ex)
        {
            // Handle error appropriately (log, show message, etc.)
            throw new InvalidOperationException("فشل في إكمال الطلب", ex);
        }
    }
    public async Task OrderCompletedWithDebt()
    {
        if (ListOfSales.Count == 0) return;

        try
        {
            var order = await ProcessOrderAsync();
            _currentOrder = order;

            ClearOrder();
            await LoadProductList();
        }
        catch (Exception ex)
        {
            // Handle error appropriately (log, show message, etc.)
            throw new InvalidOperationException("فشل في إكمال الطلب", ex);
        }
    }

    public async Task<bool?> ShowDebtDialogAsync()
    {
        if (_currentOrder == null) return false;

        var debtDialogVM = new NewDebtOrderDialogViewModel(_debtServices)
        {
            CurrentOrder = _currentOrder
        };

        var dialog = new NewDebtOrderDialog()
        {
            DataContext = debtDialogVM
        };

        return await _dialogService.ShowDialogAsync(dialog);
    }
    
    
    #endregion

    #region Private Helper Methods
    private void SetUnitEnabledState(List<string> productUnitNames)
    {
        if (!productUnitNames.Any()) return;
        IsPieceEnabled = productUnitNames.Contains(PIECE_UNIT);
        IsBoxEnabled = productUnitNames.Contains(BOX_UNIT);
        IsCartonEnabled = productUnitNames.Contains(CARTON_UNIT);
        IsUnitsEnabled = productUnitNames.Contains(KILO_UNIT) || productUnitNames.Contains(GRAM_UNIT);
    }

    private bool IsQuantityValid(int quantity)
    {
        if (_selectedProduct == null) return false;

        return _selectedUnit switch
        {
            GRAM_UNIT => (quantity / GRAM_TO_KILO_FACTOR) <= _selectedProduct.MaxQuantity,
            KILO_UNIT => quantity <= _selectedProduct.MaxQuantity,
            PIECE_UNIT => quantity <= _selectedProduct.MaxQuantity,
            _ => true
        };
    }

    private bool IsQuantityValidForUnit(int quantity)
    {
        if (_selectedProduct == null) return false;

        return _selectedProduct.UnitName switch
        {
            GRAM_UNIT => (quantity / GRAM_TO_KILO_FACTOR) <= _selectedProduct.MaxQuantity,
            _ => quantity <= _selectedProduct.MaxQuantity
        };
    }

    private void ResetToCurrentQuantity()
    {
        if (_selectedProduct != null)
        {
            Quantity = _selectedProduct.Quantity;
        }
    }

    private void UpdateProductQuantity(int newQuantity)
    {
        if (_selectedProduct == null) return;

        Quantity = newQuantity;
        _selectedProduct.Quantity = newQuantity;
        RecalculateTotalPrice();
        RefreshSalesData();
    }

    private void HandlePoundUnitChange(int newQuantity)
    {
        if (_selectedProduct == null) return;

        Quantity = newQuantity;
        _selectedProduct.Quantity = CalculateQuantityFromPrice(newQuantity, _selectedProduct.UnitPrice);
        _selectedProduct.TotalPrice = _selectedProduct.Quantity * _selectedProduct.UnitPrice;
        RefreshSalesData();
    }

    private static int CalculateQuantityFromPrice(decimal totalPrice, decimal unitPrice)
    {
        return unitPrice > 0 ? (int)(totalPrice / unitPrice) : 0;
    }

    private void RecalculateTotalPrice()
    {
        if (_selectedProduct != null)
        {
            _selectedProduct.TotalPrice = _selectedProduct.Quantity * _selectedProduct.UnitPrice;
        }
    }

    private void RefreshSalesData()
    {
        CalculateTotalAmount();
        ListOfSales = new ObservableCollection<SalesCasherModel>(ListOfSales);
    }

    private void ResetSelection()
    {
        _selectedProduct = null;
        IsPieceEnabled = false;
        IsUnitsEnabled = false;
    }

    private void ClearOrder()
    {
        ListOfSales.Clear();
        TotalAmount = 0;
        ResetSelection();
        Quantity = 0;
    }

    private async Task<SalesOrder> ProcessOrderAsync()
    {
        var order = new SalesOrder { TotalAmount = TotalAmount };

        await _unitOfWork.SalesOrders.AddAsync(order);
        await _unitOfWork.SaveAsync();

        foreach (var sale in ListOfSales)
        {
            await ProcessSaleItemAsync(sale, order.SalesOrderId);
        }

        await Task.CompletedTask;

        return order;
    }

    private async Task ProcessSaleItemAsync(SalesCasherModel sale, int orderId)
    {
        var product = await _unitOfWork.Products.GetAsync(u => u.ProductId == sale.ProductId, "UnitShares");
        if (product == null) return;

        var unit = await _unitOfWork.Units.GetAsync(u => u.Name == sale.UnitName);

        var q = CalculateQuantityIfParent(product, (UnitTypes)unit.Id);

        product.QuantityInStock -= (q * sale.Quantity);

        var salesOrderDetail = new SalesOrderDetail
        {
            ProductId = sale.ProductId,
            Quantity = sale.Quantity,
            UnitCost = sale.UnitPrice,
            SubTotal = sale.TotalPrice,
            SalesOrderId = orderId,
        };

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SalesOrderDetails.AddAsync(salesOrderDetail);

        await _unitOfWork.SaveAsync();

        await Task.CompletedTask;
    }

    private int CalculateQuantityIfParent(Product product, UnitTypes unit)
    {
        var units = product.UnitShares.OrderBy(u => u.UnitId).ToList();

        int quantity = 1;

        foreach (var pUnit in units)
        {
            if (pUnit.UnitId != (int)unit)
                quantity *= pUnit.QuantityPerParent ?? 1;
            else
                break;
        }

        return quantity; // Default case if no units found
    }

    public static int ConvertToGrams(int quantity, UnitTypes unit)
    {
        return unit == UnitTypes.Kilo ? quantity * 1000 : quantity;
    }

    public void CalculateTotalAmount()
    {
        TotalAmount = ListOfSales.Sum(s => s.TotalPrice);
    }

    public async Task AddProduct(Product product, ProductUnit productUnit)
    {
        var existingProduct = ListOfSales.FirstOrDefault(s => s.ProductId == product.ProductId && s.UnitName == productUnit.Unit.Name);

        if (existingProduct != null)
        {
            UpdateExistingProduct(existingProduct, product);
        }
        else
        {
            await AddNewProduct(product, productUnit);
        }

        CalculateTotalAmount();
    }

    private void UpdateExistingProduct(SalesCasherModel existingProduct, Product product)
    {
        if (product.QuantityInStock < existingProduct.Quantity + 1)
            return;

        existingProduct.Quantity++;
        existingProduct.TotalPrice = existingProduct.Quantity * existingProduct.UnitPrice;

        _selectedProduct = existingProduct;
        Quantity = existingProduct.Quantity;

        RefreshSalesData();
    }

    private async Task AddNewProduct(Product product, ProductUnit productUnit)
    {
        var unit = await _unitOfWork.Units.GetAsync(u => u.Id == productUnit.UnitId);

        _selectedProduct = new SalesCasherModel
        {
            ProductId = product.ProductId,
            ProductName = product.Name,
            UnitPrice = productUnit.UnitPrice,
            MaxQuantity = product.QuantityInStock,
            Quantity = 1,
            TotalPrice = productUnit.UnitPrice,
            UnitName = unit.Name
        };

        Quantity = 1;
        ListOfSales.Add(_selectedProduct);
    }
    #endregion
}