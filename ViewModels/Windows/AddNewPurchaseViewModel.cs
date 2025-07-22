using Microsoft.IdentityModel.Tokens;
using POS_ModernUI.Database.Repository.IRepository;
using POS_ModernUI.Models;
using System.Collections.ObjectModel;

namespace POS_ModernUI.ViewModels.Windows;

public partial class AddNewPurchaseViewModel: ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty]
    private string _applicationTitle = "POS_ModernUI";

    [ObservableProperty]
    private string _supplierName = string.Empty;

    [ObservableProperty]
    private string _supplierPhone = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Supplier> _suppliers;

    [ObservableProperty]
    private ObservableCollection<NewPurchaseOrderModel> _newPurchaseOrders = new();

    [ObservableProperty]
    private NewPurchaseOrderModel _selectedPurchaseModel = new();

    [ObservableProperty]
    private Supplier _selectedSupplier;

    

    public AddNewPurchaseViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        Suppliers = new(_unitOfWork.Suppliers.GetAll());

        Suppliers.Insert(0,new() { SupplierId = 0, Name = "مورد جديد" });
    }

    [RelayCommand]
    private void OnAddPurchaseItem()
    {
        NewPurchaseOrders.Add(new() { Id = NewPurchaseOrders.Count + 1});
    }

    [RelayCommand]
    private void OnDeletePurchaseOrderItem(NewPurchaseOrderModel model)
    {
        NewPurchaseOrders.Remove(model);
    }

    public void OnSavePurchases()
    {
        if (NewPurchaseOrders.IsNullOrEmpty())
            return;

        if (string.IsNullOrWhiteSpace(SelectedSupplier.Name))
            return;

        if (SelectedSupplier.SupplierId == 0)
        {
            var supplier = new Supplier()
            {
                Name = SupplierName,
                ContactNumber = SupplierPhone
            };
            _unitOfWork.Suppliers.Add(supplier);
            _unitOfWork.Save();
            Suppliers.Add(supplier);
            SelectedSupplier = supplier;
        }

        var order = new PurchaseOrder()
        {
            SupplierId = SelectedSupplier.SupplierId,
        };

        _unitOfWork.PurchaseOrders.Add(order);
        _unitOfWork.Save();

        foreach (var item in NewPurchaseOrders)
        {
            if (item.ProductName == string.Empty || item.UnitCost == 0)
                continue;

            var product = _unitOfWork.Products.Get(u => u.Barcode == item.ProductCode);
            var orderDetails = new PurchaseOrderDetail()
            {
                PurchaseOrderId = order.PurchaseOrderId,
                UnitCost = item.UnitCost
            };

            if (item.SelectedUnit == "قطعة")
                orderDetails.Quantity = item.Quantity;
            else if (item.SelectedUnit == "كيلو")
            {
                orderDetails.Quantity = item.Quantity * 1000; // Convert to grams
                orderDetails.UnitCost = item.UnitCost / 1000; // Adjust unit cost to per gram
            }
            else if (item.SelectedUnit == "جرام")
                orderDetails.Quantity = item.Quantity; // Already in grams

            orderDetails.SubTotal = item.UnitCost * item.Quantity;


            if (product != null)
            {
                orderDetails.ProductId = product.ProductId;
                product.QuantityInStock += orderDetails.Quantity;
                _unitOfWork.Products.Update(product);
            }
            else
            {
                orderDetails.Product = new Product()
                {
                    Barcode = item.ProductCode,
                    Name = item.ProductName,
                    QuantityInStock = item.Quantity,
                    UnitPrice = item.UnitCost,
                    UnitName = item.SelectedUnit,
                };
            }
            _unitOfWork.PurchaseOrderDetails.Add(orderDetails);
            _unitOfWork.Save();

            order.TotalAmount += orderDetails.SubTotal;
        }
        _unitOfWork.PurchaseOrders.Update(order);

        _unitOfWork.Save();
    }
    public void GetProductByBarCode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return;

        var product = _unitOfWork.Products.Get(u => u.Barcode == barcode);

        if (product == null)
            return;
        var items = NewPurchaseOrders;

        var item = items.FirstOrDefault(u => u.Id == SelectedPurchaseModel.Id);

        if (item == null) return;

        item.ProductName = product.Name;
        item.UnitCost = product.UnitPrice;
        item.SelectedUnit = product.UnitName;
        item.IsReadOnly = true;

        NewPurchaseOrders = new(items);
    }

    public void RestartView()
    {
        NewPurchaseOrders.Clear();
        SelectedPurchaseModel = new();
        SupplierName = string.Empty;
        SupplierPhone = string.Empty;
    }
}
