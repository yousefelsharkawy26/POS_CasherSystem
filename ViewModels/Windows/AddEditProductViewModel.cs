using POS_ModernUI.Database.Repository.IRepository;
using POS_ModernUI.Helpers;
using POS_ModernUI.Models;
using System.Collections.ObjectModel;

namespace POS_ModernUI.ViewModels.Windows;
public partial class AddEditProductViewModel: ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty]
    private string _applicationTitle = "POS_ModernUI";

    [ObservableProperty]
    private string _productName = string.Empty;
    [ObservableProperty]
    private int _productQuantity;
    [ObservableProperty]
    private decimal _productUnitPrice;
    [ObservableProperty]
    private string _productBarCode = string.Empty;
    [ObservableProperty]
    private string _ImageUrl = string.Empty;

    [ObservableProperty]
    private string _selectedUnit;

    [ObservableProperty]
    private ObservableCollection<object> _units = new()
    {
        new { Name = "قطعة" },
        new { Name = "كيلو" },
        new { Name = "جرام" },
    };

    public AddEditProductViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public bool SaveNewProduct()
    {
        var product = new Product
        {
            Name = ProductName,
            QuantityInStock = ProductQuantity,
            Barcode = ProductBarCode,
            Image = ImageUrl,
            UnitPrice = ProductUnitPrice
        };

        if (SelectedUnit == "{ Name = كيلو }")
        {
            product.UnitName = "جرام";
            product.QuantityInStock = ProductQuantity * 1000; // Convert kilograms to grams
            product.UnitPrice = ProductUnitPrice / 1000; // Adjust unit price to per gram
        }
        else if (SelectedUnit == "{ Name = جرام }")
        {
            product.UnitName = "جرام";
            product.QuantityInStock = ProductQuantity; // Keep grams as is
        }
        else
        {
            product.UnitName = "قطعة"; // Default unit
            product.QuantityInStock = ProductQuantity;
        }

        _unitOfWork.Products.Add(product);
        _unitOfWork.Save();

        Wpf.Ui.Controls.MessageBox msg = new();
        var result = msg.ShowMessage("نجاح", "تم حفظ المنتج بنجاح هل تريد اضافة منتج اخر ؟", MessageBoxButton.OKCancel);

        // Optionally, you can reset the fields after saving
        ProductName = string.Empty;
        ProductQuantity = 0;
        ProductUnitPrice = 0;
        ProductBarCode = string.Empty;
        ImageUrl = string.Empty;

        return result == Wpf.Ui.Controls.MessageBoxResult.Primary;
    }

    public void UpdateCurrentProduct(Product product)
    {
        // Logic to update the current product
        if (SelectedUnit.Equals("{ Name = كيلو }"))
        {
            ProductQuantity = ProductQuantity * 1000;
            SelectedUnit = "جرام"; // Convert kilograms to grams
            // Convert grams to kilograms if the unit is "جرام"
        }

        product.Name = ProductName;
        product.QuantityInStock = ProductQuantity;
        product.UnitPrice = ProductUnitPrice;
        product.UnitName = SelectedUnit;
        product.Barcode = ProductBarCode;
        product.Image = ImageUrl;
        _unitOfWork.Products.Update(product);
        _unitOfWork.Save();
    }
}
