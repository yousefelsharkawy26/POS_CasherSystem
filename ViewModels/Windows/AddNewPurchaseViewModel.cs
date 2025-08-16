using Microsoft.Extensions.Logging;
using POS_ModernUI.DataAccess.UnitOfWork;
using POS_ModernUI.Models;
using POS_ModernUI.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace POS_ModernUI.ViewModels.Windows;

public partial class AddNewPurchaseViewModel : ObservableObject
{
    #region Fields
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddNewPurchaseViewModel> _logger;
    private readonly ProductServices _productServices;
    #endregion

    #region Props
    [ObservableProperty] private string _applicationTitle = Application.ResourceAssembly.GetName().Name!;
    [ObservableProperty] private string _supplierName = string.Empty;
    [ObservableProperty] private string _supplierPhone = string.Empty;
    [ObservableProperty] private ObservableCollection<Supplier> _suppliers = new();
    [ObservableProperty] private ObservableCollection<NewPurchaseOrderModel> _newPurchaseOrders = new();
    [ObservableProperty] private NewPurchaseOrderModel _selectedPurchaseModel = new();
    [ObservableProperty] private Supplier? _selectedSupplier;
    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private bool _isLoading;
    #endregion

    #region Constructors
    public AddNewPurchaseViewModel(IUnitOfWork unitOfWork, 
                                   ILogger<AddNewPurchaseViewModel> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger;
        _productServices = new ProductServices(_unitOfWork);
        _ = InitializeAsync();
    }
    #endregion

    #region Commands
    [RelayCommand]
    private void OnAddPurchaseItem()
    {
        var newItem = new NewPurchaseOrderModel
        {
            Id = GenerateNextId(),
        };

        NewPurchaseOrders.Add(newItem);
        SelectedPurchaseModel = newItem;
        CalculateTotalAmount();
    }

    [RelayCommand]
    private void OnDeletePurchaseOrderItem(NewPurchaseOrderModel model)
    {
        if (model == null) return;

        NewPurchaseOrders.Remove(model);

        // Update IDs to maintain sequence
        for (int i = 0; i < NewPurchaseOrders.Count; i++)
        {
            NewPurchaseOrders[i].Id = i + 1;
        }

        if (SelectedPurchaseModel?.Id == model.Id)
        {
            SelectedPurchaseModel = NewPurchaseOrders.FirstOrDefault() ?? new();
        }

        CalculateTotalAmount();
    }

    [RelayCommand]
    private async Task OnSavePurchasesAsync()
    {
        try
        {
            IsLoading = true;

            if (!await ValidatePurchaseDataAsync())
                return;

            var supplier = await GetOrCreateSupplierAsync();
            if (supplier == null) return;

            var order = await CreatePurchaseOrderAsync(supplier);
            await ProcessPurchaseItemsAsync(order);

            await ShowSuccessMessage("تم حفظ طلب الشراء بنجاح");
            RestartView();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving purchase order");
            await ShowErrorMessage($"حدث خطأ أثناء حفظ طلب الشراء: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    #endregion

    #region Initializations
    private async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            await LoadSuppliersAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error initializing AddNewPurchaseViewModel");
            await ShowErrorMessage("حدث خطأ في تحميل البيانات");
        }
        finally
        {
            IsLoading = false;
        }
    }
    private async Task LoadSuppliersAsync()
    {
        var suppliers = (await _unitOfWork.Suppliers.GetAllAsync()).ToList();

        Suppliers.Clear();
        Suppliers.Add(new Supplier { SupplierId = 0, Name = "مورد جديد" });

        foreach (var supplier in suppliers)
        {
            Suppliers.Add(supplier);
        }
    }
    #endregion

    #region Class Helpers
    public async Task PurchaseProductAsync(
        string productName,
        ObservableCollection<NewProductUnitModel> units,
        int purchasedQuantity,
        int purchasedUnitId)
    {
        IsLoading = true;
        try
        {
            var product = new Product
            {
                Name = productName,
                UnitShares = new List<ProductUnit>()
            };

            var productUnits = new List<ProductUnit>();
            foreach (var unitModel in units)
                productUnits.Add(new ProductUnit
                {
                    UnitId = unitModel.SelectedUnit.Id,
                    UnitPrice = unitModel.UnitCost,
                    ProductQuantity = unitModel.Quantity,
                    ProductBarCode = unitModel.ProductCode,
                    IsDefault = false,
                    QuantityPerParent = unitModel.QuantityPerParent,
                    ParentProductUnitId = unitModel.ParentProductUnitId
                });

            await _productServices.PurchaseProductAsync(product, purchasedQuantity, purchasedUnitId, productUnits);

            await ShowMessageAsync("تمت إضافة الكمية بنجاح.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "خطأ أثناء شراء المنتج");
            await ShowMessageAsync("حدث خطأ أثناء شراء المنتج");
        }
        finally
        {
            IsLoading = false;
        }
    }
    public async Task SearchProductByBarcodeAsync(string SearchBarcode)
    {
        if (string.IsNullOrWhiteSpace(SearchBarcode))
        {
            await ShowErrorMessage("يرجى إدخال الباركود");
            return;
        }

        await GetProductByBarCodeAsync(SearchBarcode);
        SearchBarcode = string.Empty;
    }
    private async Task<bool> ValidatePurchaseDataAsync()
    {
        var errors = new List<string>();

        if (!NewPurchaseOrders.Any())
        {
            errors.Add("يجب إضافة عنصر واحد على الأقل للطلب");
        }

        if (SelectedSupplier == null)
        {
            errors.Add("يجب اختيار المورد");
        }
        else if (SelectedSupplier.SupplierId == 0)
        {
            if (string.IsNullOrWhiteSpace(SupplierName))
                errors.Add("يجب إدخال اسم المورد الجديد");

            if (string.IsNullOrWhiteSpace(SupplierPhone))
                errors.Add("يجب إدخال رقم هاتف المورد");
        }

        // Validate purchase items
        var invalidItems = NewPurchaseOrders.Where(item =>
            string.IsNullOrWhiteSpace(item.ProductName) ||
            item.UnitCost <= 0 ||
            item.Quantity <= 0).ToList();

        if (invalidItems.Any())
        {
            errors.Add($"يوجد {invalidItems.Count} عنصر غير صحيح في الطلب (يجب أن يحتوي على اسم المنتج، سعر، وكمية صحيحة)");
        }

        if (errors.Any())
        {
            await ShowErrorMessage($"يرجى تصحيح الأخطاء التالية:\n{string.Join("\n", errors)}");
            return false;
        }

        return true;
    }
    private async Task<Supplier?> GetOrCreateSupplierAsync()
    {
        if (SelectedSupplier == null) return null;

        if (SelectedSupplier.SupplierId != 0)
            return SelectedSupplier;

        // Create new supplier
        var newSupplier = new Supplier
        {
            Name = SupplierName.Trim(),
            ContactNumber = SupplierPhone.Trim()
        };

        await _unitOfWork.Suppliers.AddAsync(newSupplier);
        await _unitOfWork.SaveAsync();

        Suppliers.Add(newSupplier);
        SelectedSupplier = newSupplier;

        return newSupplier;
    }
    private async Task<PurchaseOrder> CreatePurchaseOrderAsync(Supplier supplier)
    {
        var order = new PurchaseOrder
        {
            SupplierId = supplier.SupplierId,
            Date = DateOnly.FromDateTime(DateTime.Now),
            TotalAmount = 0 // Will be calculated
        };

        await _unitOfWork.PurchaseOrders.AddAsync(order);
        await _unitOfWork.SaveAsync();

        return order;
    }
    private async Task ProcessPurchaseItemsAsync(PurchaseOrder order)
    {
        decimal orderTotal = 0;

        foreach (var item in NewPurchaseOrders.Where(i =>
            !string.IsNullOrWhiteSpace(i.ProductName) &&
            i.UnitCost > 0 &&
            i.Quantity > 0))
        {
            var subTotal = item.UnitCost * item.Quantity; // Use original values for subtotal

            var orderDetail = new PurchaseOrderDetail
            {
                PurchaseOrderId = order.PurchaseOrderId,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
                SubTotal = subTotal
            };

            await HandleProductForOrderDetailAsync(orderDetail, item);

            await _unitOfWork.PurchaseOrderDetails.AddAsync(orderDetail);
            orderTotal += subTotal;
        }
        order.TotalAmount = orderTotal;

        await _unitOfWork.PurchaseOrders.UpdateAsync(order);
        await _unitOfWork.SaveAsync();
    }
    private async Task HandleProductForOrderDetailAsync(PurchaseOrderDetail orderDetail, NewPurchaseOrderModel item)
    {
        Product? existingProduct = null;
        ProductUnit? existingProductUnit = null;

        // Try to find existing product by barcode
        if (!string.IsNullOrWhiteSpace(item.ProductCode))
        {
            existingProductUnit = await 
                _unitOfWork.ProductUnits
                .GetAsync(p => p.ProductBarCode == item.ProductCode, "Product");

            existingProduct = existingProductUnit.Product;
        }

        if (existingProduct != null)
        {
            // Update existing product
            orderDetail.ProductId = existingProduct.ProductId;


            existingProduct.QuantityInStock = await StockQuantityFromBaseType(orderDetail.ProductId,
                                                                              item,
                                                                              orderDetail.Quantity,
                                                                              (UnitTypes)item.SelectedUnit.Id);

            await _unitOfWork.Products.UpdateAsync(existingProduct);
        }
        else
        {
            // Create new product
            var newProduct = new Product
            {
                Name = item.ProductName.Trim(),
                QuantityInStock = orderDetail.Quantity,
                UnitShares = new List<ProductUnit>()
            };

            var newProductUnit = new ProductUnit
            {
                Product = newProduct,
                ProductBarCode = item.ProductCode?.Trim() ?? string.Empty,
                UnitPrice = orderDetail.UnitCost,
                ProductQuantity = orderDetail.Quantity,
                IsDefault = true,
                Unit = item.SelectedUnit
            };

            newProduct.UnitShares.Add(newProductUnit);
            orderDetail.Product = newProduct;
        }
    }
    private async Task<int> StockQuantityFromBaseType(int productId, NewPurchaseOrderModel item, int quantity, UnitTypes unit)
    {
        var product = await _unitOfWork.Products.GetAsync(u => u.ProductId == productId, "UnitShares.Unit");
        var carton = product.UnitShares.FirstOrDefault(u => (UnitTypes)u.UnitId == UnitTypes.Carton);
        var box = product.UnitShares.FirstOrDefault(u => (UnitTypes)u.UnitId == UnitTypes.Box);
        var piece = product.UnitShares.FirstOrDefault(u => (UnitTypes)u.UnitId == UnitTypes.Piece);
        var kilo = product.UnitShares.FirstOrDefault(u => (UnitTypes)u.UnitId == UnitTypes.Kilo);
        var gram = product.UnitShares.FirstOrDefault(u => (UnitTypes)u.UnitId == UnitTypes.Gram);

        int result = 0;

        switch (unit)
        {
            case UnitTypes.Carton:
                // إذا كان لدينا علبة وقطعة
                if (carton != null)
                {
                    if (box != null && piece != null)
                    {
                        var bQuantity = (box.ProductQuantity == 0 ? 1 : box.ProductQuantity) * (box.QuantityPerParent ?? 1);
                        var pQuantity = (piece.ProductQuantity == 0 ? 1 : piece.ProductQuantity) * (piece.QuantityPerParent ?? 1);
                        result = quantity * bQuantity * pQuantity;
                    }
                    else if (box != null)
                    {
                        var bQuantity = (box.ProductQuantity == 0 ? 1 : box.ProductQuantity) * (box.QuantityPerParent ?? 1);
                        result = quantity * bQuantity;
                    }
                    else if (piece != null)
                    {
                        var pQuantity = (piece.ProductQuantity == 0 ? 1 : piece.ProductQuantity) * (piece.QuantityPerParent ?? 1);
                        result = quantity * pQuantity;
                    }
                    else
                        result = quantity;
                }
                else
                {
                    // إضافة كرتونة جديدة
                    carton = new ProductUnit
                    {
                        ProductId = productId,
                        ProductBarCode = item.ProductCode,
                        UnitId = (int)unit,
                        UnitPrice = item.UnitCost,
                        ProductQuantity = 1
                    };
                    await _unitOfWork.ProductUnits.AddAsync(carton);
                    result = quantity;
                }
                break;

            case UnitTypes.Box:
                if (box != null)
                {
                    if (piece != null)
                    {
                        var pQuantity = (piece.ProductQuantity == 0 ? 1 : piece.ProductQuantity) * (piece.QuantityPerParent ?? 1);
                        result = quantity * pQuantity;
                    }
                    else
                        result = quantity;
                }
                else
                {
                    box = new ProductUnit
                    {
                        ProductId = productId,
                        ProductBarCode = item.ProductCode,
                        UnitId = (int)unit,
                        UnitPrice = item.UnitCost,
                        ProductQuantity = 1
                    };
                    await _unitOfWork.ProductUnits.AddAsync(box);
                    result = quantity;
                }
                break;

            case UnitTypes.Piece:
                result = quantity;
                break;

            case UnitTypes.Kilo:
                // التحويل للجرام
                result = quantity * 1000;
                break;

            case UnitTypes.Gram:
                result = quantity;
                break;

            default:
                result = quantity;
                break;
        }
        // commit changes
        await _unitOfWork.SaveAsync();
        // إضافة الكمية للمخزون الحالي
        return result + product.QuantityInStock;
    }
    public async Task GetProductByBarCodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode) || SelectedPurchaseModel == null)
            return;

        try
        {
            // ابحث عن ProductUnit بالباركود مع تضمين المنتج والوحدة
            var productUnit = await _unitOfWork.ProductUnits.GetAsync(
                    pu => pu.ProductBarCode == barcode,
                    "Product,Unit");

            if (productUnit == null || productUnit.Product == null)
            {
                await ShowWarningMessage("لم يتم العثور على المنتج بهذا الباركود");
                return;
            }

            var item = NewPurchaseOrders.FirstOrDefault(i => i.Id == SelectedPurchaseModel.Id);
            if (item == null) return;

            // تحديث بيانات العنصر المختار
            item.ProductCode = barcode;
            item.ProductName = productUnit.Product.Name ?? string.Empty;
            item.UnitCost = productUnit.UnitPrice;
            item.SelectedUnit = productUnit.Unit;
            item.IsReadOnly = true;

            // تحديث الواجهة
            OnPropertyChanged(nameof(NewPurchaseOrders));
            CalculateTotalAmount();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error retrieving product by barcode: {Barcode}", barcode);
            await ShowErrorMessage("حدث خطأ أثناء البحث عن المنتج");
        }
    }
    private void CalculateTotalAmount()
    {
        TotalAmount = NewPurchaseOrders
            .Where(item => item.UnitCost > 0 && item.Quantity > 0)
            .Sum(item => item.UnitCost * item.Quantity);
    }
    private int GenerateNextId()
    {
        return NewPurchaseOrders.Any() ? NewPurchaseOrders.Max(x => x.Id) + 1 : 1;
    }
    public async void RestartView()
    {
        NewPurchaseOrders.Clear();
        SelectedPurchaseModel = new();
        SupplierName = string.Empty;
        SupplierPhone = string.Empty;
        await LoadSuppliersAsync();
        TotalAmount = 0;
        IsLoading = false;
    }
    partial void OnNewPurchaseOrdersChanged(ObservableCollection<NewPurchaseOrderModel> value)
    {
        // إلغاء الاشتراك من العناصر القديمة (إن وجدت)
        if (_newPurchaseOrders != null)
        {
            foreach (var item in _newPurchaseOrders)
            {
                item.PropertyChanged -= OnPurchaseItemPropertyChanged;
            }
        }

        // الاشتراك في العناصر الجديدة
        if (value != null)
        {
            foreach (var item in value)
            {
                item.PropertyChanged += OnPurchaseItemPropertyChanged;
            }
        }

        // إعادة حساب الإجمالي
        CalculateTotalAmount();
    }

    private void OnPurchaseItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NewPurchaseOrderModel.UnitCost) ||
            e.PropertyName == nameof(NewPurchaseOrderModel.Quantity))
        {
            CalculateTotalAmount();
        }
    }

    // UI Helper Methods
    private async Task ShowSuccessMessage(string message)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox();
        messageBox.Content = message;
        messageBox.Title = "نجاح";
        messageBox.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Transparent;
        messageBox.PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary;
        await messageBox.ShowDialogAsync();
    }
    private async Task ShowErrorMessage(string message)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox();
        messageBox.Content = message;
        messageBox.Title = "خطأ";
        messageBox.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Transparent;
        messageBox.PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary;
        await messageBox.ShowDialogAsync();
    }
    private async Task ShowWarningMessage(string message)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox();
        messageBox.Content = message;
        messageBox.Title = "تحذير";
        messageBox.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Transparent;
        messageBox.PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary;
        await messageBox.ShowDialogAsync();
    }
    private async Task ShowMessageAsync(string message)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Content = message,
            Title = "تنبيه",
            CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Transparent,
            PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary
        };
        await messageBox.ShowDialogAsync();
    }
    #endregion
}