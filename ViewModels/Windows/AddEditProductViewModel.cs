using Microsoft.Extensions.Logging;
using POS_ModernUI.DataAccess.UnitOfWork;
using POS_ModernUI.Helpers;
using POS_ModernUI.Services;
using POS_ModernUI.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace POS_ModernUI.ViewModels.Windows;

public enum UnitTypes
{
    Piece = 1,
    Box = 2,
    Carton = 3,
    Kilo = 4,
    Gram = 5
}

public partial class AddEditProductViewModel : ObservableObject
{
    #region Fields
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddEditProductViewModel> _logger;
    private readonly ProductServices _productServices;
    #endregion

    #region Props
    [ObservableProperty] private string _applicationTitle = Application.ResourceAssembly.GetName().Name!;
    [ObservableProperty] private string _productName = string.Empty;
    [ObservableProperty] private string _imageUrl = string.Empty;
    [ObservableProperty] private ObservableCollection<NewProductUnitModel> _newProductUnits = new();
    [ObservableProperty] private NewProductUnitModel _selectedProductUnit = new();
    [ObservableProperty] private bool _isEditMode = false;
    [ObservableProperty] private bool _isBusy = false;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _quantityUnitType = "إضافة كمية";
    #endregion

    #region Constructors
    public AddEditProductViewModel(IUnitOfWork unitOfWork, ILogger<AddEditProductViewModel> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _productServices = new ProductServices(_unitOfWork);
    }
    #endregion

    #region Initialization
    public async Task LoadProduct(int productId)
    {
        IsBusy = true;
        try
        {
            var product = await _unitOfWork.Products.GetAsync(p => p.ProductId == productId, "UnitShares.Unit");

            if (product == null)
            {
                await ShowErrorMessage("المنتج غير موجود");
                return;
            }

            var unitQuantities = product.UnitShares
                .Select(u => u.QuantityPerParent)
                .ToList();

            var quantities = new List<int>();

            var unitCount = product.UnitShares.Count;

            for (int i = 0; i < unitCount; i++)
            {
                var quantity = unitQuantities
                .Aggregate((a, b) =>
                {
                    if (a == null && b == null) return 0;
                    if (a == null && b != null) return b;
                    if (b == null && a != null) return a;
                    return a * b;
                });

                quantities.Add(quantity ?? 1);

                unitQuantities.Remove(unitQuantities[0]);

                if (unitQuantities.Count == 0)
                    break;
                // إذا كانت القائمة فارغة، نضع null لتجنب الأخطاء
                unitQuantities[0] = null;
            }

            var stockQuantity = product.QuantityInStock;

            NewProductUnits = new ObservableCollection<NewProductUnitModel>(
                product.UnitShares
                    .OrderByDescending(u => u.ParentProductUnitId == null) // الأكبر أولاً
                    .Select((u, i) =>
                    {
                        var pu = new NewProductUnitModel(u)
                        {
                            Id = i + 1,
                            IsFirstRow = (i == 0),
                            QuantityPerParent = u.QuantityPerParent,
                            Quantity = stockQuantity / quantities[i],
                        };

                        stockQuantity %= quantities[i]; // تحديث الكمية المتبقية

                        return pu;
                    })
            );

            ProductName = product.Name;
            ImageUrl = product.Image;
            IsEditMode = true;
            UpdateIsFirstRowFlags();
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region Commands
    [RelayCommand]
    private async Task OnAddProductUnitItem()
    {
        if (!await ValidateAddProductFields(IsEditMode))
            return;

        var newItem = new NewProductUnitModel
        {
            Id = GenerateNextId(),
            IsFirstRow = NewProductUnits.Count == 0
        };

        var lastItem = NewProductUnits.LastOrDefault();

        if (lastItem != null)
        {
            if (lastItem.SelectedUnit.Id == (int)UnitTypes.Carton)
            {
                newItem.Units.Remove(newItem.Units.FirstOrDefault(u => u.Id == (int)UnitTypes.Carton));
                newItem.Units.Remove(newItem.Units.FirstOrDefault(u => u.Id == (int)UnitTypes.Kilo));
                newItem.Units.Remove(newItem.Units.FirstOrDefault(u => u.Id == (int)UnitTypes.Gram));
            }
            else if (lastItem.SelectedUnit.Id == (int)UnitTypes.Box)
            {
                newItem.Units.Remove(newItem.Units.FirstOrDefault(u => u.Id == (int)UnitTypes.Box));
                newItem.Units.Remove(newItem.Units.FirstOrDefault(u => u.Id == (int)UnitTypes.Carton));
                newItem.Units.Remove(newItem.Units.FirstOrDefault(u => u.Id == (int)UnitTypes.Kilo));
                newItem.Units.Remove(newItem.Units.FirstOrDefault(u => u.Id == (int)UnitTypes.Gram));
            }
            else
            {
                return; 
            }
        }

        NewProductUnits.Add(newItem);
        SelectedProductUnit = newItem;
        UpdateIsFirstRowFlags();

        QuantityUnitType = NewProductUnits.Count > 0 ? "إضافة وحدة أصغر" : "إضافة كمية";
    }

    [RelayCommand]
    private void OnDeleteProductUnitItem(NewProductUnitModel model)
    {
        if (model == null) return;

        NewProductUnits.Remove(model);

        // Update IDs to maintain sequence
        for (int i = 0; i < NewProductUnits.Count; i++)
        {
            NewProductUnits[i].Id = i + 1;
        }

        SelectedProductUnit = NewProductUnits.FirstOrDefault() ?? new();
        UpdateIsFirstRowFlags();

        QuantityUnitType = NewProductUnits.Count > 0 ? "إضافة وحدة أصغر" : "إضافة كمية";
    }

    [RelayCommand]
    public void OnResetFields()
    {
        ProductName = string.Empty;
        ImageUrl = string.Empty;
        NewProductUnits.Clear();
        SelectedProductUnit = new();
        ErrorMessage = string.Empty;
        IsEditMode = false;
    }
    #endregion

    #region Class Actions
    public async Task<bool> SaveNewProduct()
    {
        if (!await ValidateProductUnits())
        {
            await ShowErrorMessage("البيانات غير صحيحة أو هناك وحدات مكررة أو قيم غير منطقية.");
            return true;
        }

        IsBusy = true;
        try
        {
            var product = new Product
            {
                Name = ProductName,
                Image = ImageUrl,
                UnitShares = new List<ProductUnit>()
            };

            var units = BuildProductUnitsForSave(product);
            SetDefaultUnitAndStock(product, units);

            await _productServices.SaveOrUpdateProductAsync(product, units);

            var msg = new Wpf.Ui.Controls.MessageBox();
            var result = await msg.ShowMessageAsync("نجاح", "تم حفظ المنتج بنجاح هل تريد اضافة منتج اخر ؟", MessageBoxButton.OKCancel);

            OnResetFields();
            return result == Wpf.Ui.Controls.MessageBoxResult.Primary;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "خطأ أثناء حفظ المنتج");
            await ShowErrorMessage("حدث خطأ أثناء حفظ المنتج");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task UpdateCurrentProduct(Product product)
    {
        if (product == null) return;

        if (!await ValidateProductUnits(true))
        {
            await ShowErrorMessage("البيانات غير صحيحة أو هناك وحدات مكررة أو قيم غير منطقية.");
            return;
        }

        IsBusy = true;
        try
        {
            product.Name = ProductName;
            product.Image = ImageUrl;

            var units = BuildProductUnitsForSave(product);
            SetDefaultUnitAndStock(product, units);

            await _productServices.SaveOrUpdateProductAsync(product, units);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "خطأ أثناء تحديث المنتج");
            await ShowErrorMessage("حدث خطأ أثناء تحديث المنتج");
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region Method Helpers
    private void UpdateIsFirstRowFlags()
    {
        for (int i = 0; i < NewProductUnits.Count; i++)
            NewProductUnits[i].IsFirstRow = (i == 0);
    }
    private List<ProductUnit> BuildProductUnitsForSave(Product product)
    {
        var units = new List<ProductUnit>();
        int? parentUnitId = null;
        for (int i = 0; i < NewProductUnits.Count; i++)
        {
            var model = NewProductUnits[i];
            var pu = new ProductUnit
            {
                UnitId = model.SelectedUnit.Id,
                UnitPrice = model.UnitCost,
                ProductQuantity = model.Quantity,
                ProductBarCode = model.ProductCode,
                ParentProductUnitId = parentUnitId,
                QuantityPerParent = i == 0 ? null : model.QuantityPerParent,
                ProductId = product.ProductId > 0 ? product.ProductId : 0,
                Product = product.ProductId > 0 ? null : product
            };
            units.Add(pu);
            parentUnitId = pu.UnitId; // أو استخدم معرف داخلي لو متاح
        }
        return units;
    }
    private async Task<bool> ValidateAddProductFields(bool isUpdate = false)
    {
        if (string.IsNullOrWhiteSpace(ProductName))
        {
            ErrorMessage = "يرجى إدخال اسم المنتج.";
            return false;
        }

        if (NewProductUnits.Any(u => u.SelectedUnit == null || u.Quantity < 0 || u.UnitCost <= 0))
        {
            ErrorMessage = "يرجى التأكد من ملء جميع الحقول بشكل صحيح.";
            return false;
        }

        if (NewProductUnits.GroupBy(u => u.SelectedUnit.Id).Any(g => g.Count() > 1))
        {
            ErrorMessage = "لا يمكن إضافة نفس الوحدة أكثر من مرة.";
            return false;
        }

        // تحقق من أن كل صف بعد الأول له QuantityPerParent > 0
        for (int i = 1; i < NewProductUnits.Count; i++)
        {
            if (NewProductUnits[i].QuantityPerParent == null || NewProductUnits[i].QuantityPerParent <= 0)
            {
                ErrorMessage = "يرجى إدخال عدد الوحدات في الوحدة الأكبر لكل وحدة أصغر.";
                return false;
            }
        }

        if (isUpdate)
            return true;

        var lastUnit = NewProductUnits.LastOrDefault();

        if (lastUnit != null && await _unitOfWork.ProductUnits.GetAsync(u => u.ProductBarCode == lastUnit.ProductCode) != null)
        {
            ErrorMessage = "يوجد باركود مكرر في الوحدات.";
            return false;
        }

        return true;
    }
    private async Task<bool> ValidateProductUnits(bool isUpdate = false)
    {
        if (!NewProductUnits.Any())
            return false;

        // منع تكرار نفس الوحدة
        var duplicate = NewProductUnits.GroupBy(u => u.SelectedUnit.Id).Any(g => g.Count() > 1);
        if (duplicate)
        {
            ErrorMessage = "لا يمكن إضافة نفس الوحدة أكثر من مرة.";
            return false;
        }
        // تحقق من القيم السالبة أو الفارغة
        if (NewProductUnits.Any(u => u.Quantity < 0 || u.UnitCost <= 0))
        {
            ErrorMessage = "يرجى التأكد من ملء جميع الحقول بشكل صحيح.";
            return false;
        }

        // تحقق من أن كل صف بعد الأول له QuantityPerParent > 0
        for (int i = 1; i < NewProductUnits.Count; i++)
        {
            if (NewProductUnits[i].QuantityPerParent == null || NewProductUnits[i].QuantityPerParent <= 0)
            {
                ErrorMessage = "يرجى إدخال عدد الوحدات في الوحدة الأكبر لكل وحدة أصغر.";
                return false;
            }
        }

        if (isUpdate)
            return true;

        // تحقق من الباركود المكرر في قاعدة البيانات
        foreach (var unit in NewProductUnits)
        {
            var exists = await _unitOfWork.ProductUnits.GetAsync(pu => pu.ProductBarCode == unit.ProductCode) != null;
            if (!string.IsNullOrWhiteSpace(unit.ProductCode) && exists)
            {
                ErrorMessage = "يوجد باركود مكرر في الوحدات.";
                return false;
            }
        }

        return true;
    }
    private void SetDefaultUnitAndStock(Product product, List<ProductUnit>? units)
    {
        var carton = units?.FirstOrDefault(u => (UnitTypes)u.UnitId == UnitTypes.Carton);
        var box = units?.FirstOrDefault(u => (UnitTypes)u.UnitId == UnitTypes.Box);
        var piece = units?.FirstOrDefault(u => (UnitTypes)u.UnitId == UnitTypes.Piece);

        if (carton != null)
        {
            var cQuantity = (carton.ProductQuantity == 0 ? 1 : carton.ProductQuantity) * (carton.QuantityPerParent ?? 1);
            if (box != null)
            {
                var bQuantity = (box.ProductQuantity == 0 ? 1 : box.ProductQuantity) * (box.QuantityPerParent ?? 1);
                if (piece != null)
                {
                    piece.IsDefault = true;
                    carton.IsDefault = false;
                    box.IsDefault = false;
                    var pQuantity = (piece.ProductQuantity == 0 ? 1 : piece.ProductQuantity) * (piece.QuantityPerParent ?? 1);
                    product.QuantityInStock = (cQuantity * bQuantity * pQuantity);
                }
                else
                {
                    box.IsDefault = true;
                    carton.IsDefault = false;
                    product.QuantityInStock = (cQuantity * bQuantity);
                }
            }
            else if (piece != null)
            {
                piece.IsDefault = true;
                carton.IsDefault = false;
                var pQuantity = (piece.ProductQuantity == 0 ? 1 : piece.ProductQuantity) * (piece.QuantityPerParent ?? 1);
                product.QuantityInStock = (cQuantity * pQuantity);
            }
            else
            {
                carton.IsDefault = true;
                product.QuantityInStock = cQuantity;
            }
        }
        else if (box != null)
        {
            var bQuantity = (box.ProductQuantity == 0 ? 1 : box.ProductQuantity) * (box.QuantityPerParent ?? 1);
            if (piece != null)
            {
                piece.IsDefault = true;
                box.IsDefault = false;
                var pQuantity = (piece.ProductQuantity == 0 ? 1 : piece.ProductQuantity) * (piece.QuantityPerParent ?? 1);
                product.QuantityInStock = (bQuantity * pQuantity);
            }
            else
            {
                box.IsDefault = true;
                product.QuantityInStock = box.ProductQuantity;
            }
        }
        else if (piece != null)
        {
            piece.IsDefault = true;
            product.QuantityInStock = piece.ProductQuantity;
        }
        else
        {
            var first = units?.FirstOrDefault();
            if (first != null)
            {
                first.IsDefault = true;
                var quantity = ConvertToGrams(first.ProductQuantity, (UnitTypes)first.UnitId);
                product.QuantityInStock = quantity;
            }
        }
    }
    public static int ConvertToGrams(int quantity, UnitTypes unit)
    {
        return unit == UnitTypes.Kilo ? quantity * 1000 : quantity;
    }
    public async Task SearchProductByBarcodeAsync(string searchBarcode)
    {
        if (string.IsNullOrWhiteSpace(searchBarcode))
        {
            await ShowErrorMessage("يرجى إدخال الباركود");
            return;
        }

        await GetProductByBarCodeAsync(searchBarcode);
    }
    public async Task GetProductByBarCodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode) || SelectedProductUnit == null)
            return;

        try
        {
            var productUnit = await
                _unitOfWork.ProductUnits.GetAsync(
                    pu => pu.ProductBarCode == barcode,
                    "Product,Unit"
                );

            if (productUnit != null && productUnit.Product != null)
            {
                await ShowWarningMessage("هذا المنتج موجود بالفعل ولا يمكن إضافته مرة أخرى.");
                return;
            }
            // يمكنك هنا تفعيل واجهة الإدخال أو إعادة تعيين الحقول إذا أردت
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error retrieving product by barcode: {Barcode}", barcode);
            await ShowErrorMessage("حدث خطأ أثناء البحث عن المنتج");
        }
    }
    private int GenerateNextId()
    {
        return NewProductUnits.Any() ? NewProductUnits.Max(x => x.Id) + 1 : 1;
    }
    // UI Helper Methods
    private async Task ShowErrorMessage(string message)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Content = message,
            Title = "خطأ",
            CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Transparent,
            PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary
        };
        await messageBox.ShowDialogAsync();
    }
    private async Task ShowWarningMessage(string message)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Content = message,
            Title = "تحذير",
            CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Transparent,
            PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary
        };
        await messageBox.ShowDialogAsync();
    }
    #endregion
}