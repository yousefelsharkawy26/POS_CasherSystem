using Microsoft.IdentityModel.Tokens;
using POS_ModernUI.Database.Repository.IRepository;
using POS_ModernUI.Helpers;
using POS_ModernUI.Models;
using POS_ModernUI.Services.Contracts;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace POS_ModernUI.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IUnitOfWork _unitOfWork;
        private INotificationService _notificationService;
        private SalesCasherModel? _selectedProduct;

        [ObservableProperty]
        private bool _isWithoutBarCode = false;

        [ObservableProperty]
        private decimal _totalAmount = 0;

        [ObservableProperty]
        private int _quantity;

        [ObservableProperty]
        private bool _isPieceEnabled = false;

        [ObservableProperty]
        private bool _isUnitsEnabled = false;

        [ObservableProperty]
        private ObservableCollection<SalesCasherModel> _listOfSales;

        [ObservableProperty]
        private ObservableCollection<Product> _productList;

        public string SalesOrderBarcode { get; set; }

        public DashboardViewModel(IUnitOfWork unitOfWork, 
                                  INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            ListOfSales = new ObservableCollection<SalesCasherModel>();
            ProductList = new(_unitOfWork.Products.GetAll(u => u.QuantityInStock > 0).OrderBy(u => u.Name));
            _notificationService = notificationService;
        }

        partial void OnQuantityChanged(int value)
        {
            if (_selectedProduct == null)
                return;

            if (value < 0)
            {
                Quantity = _selectedProduct.Quantity; // Reset to old value if new value is negative
                return; // Prevent negative quantity
            }

            if (value > _selectedProduct.MaxQuantity && _selectedProduct.UnitName == "كيلو")
            {
                Quantity = _selectedProduct.Quantity; // Reset to old value if new value is invalid
                return; // Prevent negative quantity
            }

            if (value > _selectedProduct.MaxQuantity && _selectedProduct.UnitName == "قطعة")
            {
                Quantity = _selectedProduct.Quantity; // Reset to old value if new value is invalid
                return; // Prevent negative quantity
            }

            if (_selectedProduct.UnitName == "جرام" && value / 1000 > _selectedProduct.MaxQuantity)
            {
                Quantity = value; // Reset to old value if new value is invalid
                return; // Prevent adding more than the maximum quantity
            }


            if (_selectedProduct.UnitName == "جرام" && value / 1000 < _selectedProduct.MaxQuantity)
            {
                Quantity = value;
                _selectedProduct.Quantity = Quantity;
                _selectedProduct.TotalPrice = _selectedProduct.Quantity * _selectedProduct.UnitPrice;
                TotalAmount = ListOfSales.Sum(s => s.TotalPrice);
                ListOfSales = new(ListOfSales); // Refresh the collection to notify the UI of changes
            }
            else if (value > _selectedProduct.MaxQuantity)
                return; // Prevent adding more than the maximum quantity
            else
            {
                Quantity = value;
                _selectedProduct.Quantity = Quantity;
                _selectedProduct.TotalPrice = _selectedProduct.Quantity * _selectedProduct.UnitPrice;
                TotalAmount = ListOfSales.Sum(s => s.TotalPrice);
                ListOfSales = new(ListOfSales); // Refresh the collection to notify the UI of changes
            }
        }

        [RelayCommand]
        private void OnAddProduct(Product product)
        {
            // This method is called when a product is added to the order.
            if (product == null) return;

            if (product.UnitName == "قطعة")
            {
                IsPieceEnabled = true;
                IsUnitsEnabled = false;
            }
            else if (product.UnitName == "كيلو")
            {
                IsPieceEnabled = false;
                IsUnitsEnabled = true;
            }
            else if (product.UnitName == "جرام")
            {
                IsPieceEnabled = false;
                IsUnitsEnabled = true;
            }

            addProduct(product);
        }

        [RelayCommand]
        private void OnToggleChecked()
        {
            // This method is called when the toggle button is checked or unchecked.
            if (IsWithoutBarCode)
            {
                // If the toggle is checked, we can add products without a barcode.
                // You can implement the logic to handle this case.
                ProductList = new(_unitOfWork.Products.GetAll(u => string.IsNullOrEmpty(u.Barcode) && u.QuantityInStock > 0));
            }
            else
            {
                // If the toggle is unchecked, we can only add products with a barcode.
                // You can implement the logic to handle this case.
                ProductList = new(_unitOfWork.Products.GetAll(u => u.QuantityInStock > 0));
            }
        }

        [RelayCommand]
        private void OnChangeQuantity(object content)
        {
            if (_selectedProduct == null)
                return;

            if (int.TryParse(content.ToString(), out int result))
            {
                if (_selectedProduct.UnitName == "جرام" && ((Quantity * 10) + result) / 1000 < _selectedProduct.MaxQuantity)
                {
                    Quantity = (Quantity * 10) + result;
                    _selectedProduct.Quantity = Quantity;
                    _selectedProduct.TotalPrice = _selectedProduct.Quantity * _selectedProduct.UnitPrice;
                    TotalAmount = ListOfSales.Sum(s => s.TotalPrice);
                    ListOfSales = new(ListOfSales); // Refresh the collection to notify the UI of changes
                }
                else if ((Quantity * 10) + result > _selectedProduct.MaxQuantity)
                    return; // Prevent adding more than the maximum quantity
                else
                {
                    Quantity = (Quantity * 10) + result;
                    _selectedProduct.Quantity = Quantity;
                    _selectedProduct.TotalPrice = _selectedProduct.Quantity * _selectedProduct.UnitPrice;
                    TotalAmount = ListOfSales.Sum(s => s.TotalPrice);
                    ListOfSales = new(ListOfSales); // Refresh the collection to notify the UI of changes
                }
            }
            else if (content.ToString() == "قطعة")
            {
                _selectedProduct.TotalPrice = _selectedProduct.Quantity * _selectedProduct.UnitPrice;
                _selectedProduct.UnitName = "قطعة";
                TotalAmount = ListOfSales.Sum(s => s.TotalPrice);
                ListOfSales = new(ListOfSales);
            }
            else if (content.ToString() == "كيلو")
            {
                if (_selectedProduct.UnitName == "كيلو")
                    return; // Prevent changing unit if already in grams

                _selectedProduct.UnitPrice = _selectedProduct.UnitPrice * 1000; // Convert to grams
                _selectedProduct.TotalPrice = _selectedProduct.Quantity * _selectedProduct.UnitPrice;
                _selectedProduct.UnitName = "كيلو";
                TotalAmount = ListOfSales.Sum(s => s.TotalPrice);
                ListOfSales = new(ListOfSales);
            }
            else if (content.ToString() == "جرام")
            {
                if (_selectedProduct.UnitName == "جرام")
                    return; // Prevent changing unit if already in grams

                _selectedProduct.UnitPrice = _selectedProduct.UnitPrice / 1000;
                _selectedProduct.TotalPrice = _selectedProduct.Quantity * _selectedProduct.UnitPrice;
                _selectedProduct.UnitName = "جرام";
                TotalAmount = ListOfSales.Sum(s => s.TotalPrice);
                ListOfSales = new(ListOfSales);
            }
            else if (((SymbolIcon)content).Symbol == SymbolRegular.Eraser24)
            {
                if (_selectedProduct.Quantity > 0)
                {
                    _selectedProduct.Quantity = Quantity / 10;
                    _selectedProduct.TotalPrice = _selectedProduct.Quantity * _selectedProduct.UnitPrice;
                    Quantity = (Quantity / 10);
                    TotalAmount = ListOfSales.Sum(s => s.TotalPrice);
                    ListOfSales = new(ListOfSales); // Refresh the collection to notify the UI of changes
                }
            }
        }

        [RelayCommand]
        private void OnRemoveProduct(SalesCasherModel salesCasherModel)
        {
            if (salesCasherModel == null) return;
            ListOfSales.Remove(salesCasherModel);
            TotalAmount -= salesCasherModel.TotalPrice;
            if (ListOfSales.Count == 0)
            {
                _selectedProduct = null;
            }
        }

        [RelayCommand]
        private void OnOrderCanceled()
        {
            ListOfSales.Clear();
            TotalAmount = 0;
            _selectedProduct = null;
        }

        [RelayCommand]
        private void OnOrderCompleted()
        {
            // This method is called when the order is completed.
            // You can implement the logic to save the order.

            // No sales to process
            if (ListOfSales.Count == 0)
                return;

            var order = new SalesOrder
            {
                TotalAmount = this.TotalAmount,
                SalesOrderBarcode = SalesOrderBarcode
            };

            _unitOfWork.SalesOrders.Add(order);
            _unitOfWork.Save();

            foreach (var sale in ListOfSales)
            {
                var product = _unitOfWork.Products.Get(u => u.ProductId == sale.ProductId);
                product.QuantityInStock -= sale.Quantity;

                var salesOrderDetail = new SalesOrderDetail
                {
                    ProductId = sale.ProductId,
                    Quantity = sale.Quantity,
                    UnitCost = sale.UnitPrice,
                    SubTotal = sale.TotalPrice,
                    SalesOrderId = order.SalesOrderId,
                    
                };
                _unitOfWork.Products.Update(product);
                _unitOfWork.SalesOrderDetails.Add(salesOrderDetail);
                _unitOfWork.Save();

                if (product.QuantityInStock == 0)
                {
                    _notificationService.SetNotification(new("تحذير", $"تم نفاذ كل الكمية من منتج -> {product.Name}"));
                }
                else if (product.QuantityInStock < 10)
                {
                    _notificationService.SetNotification(new("تحذير", $"المنتج {product.Name} على وشك النفاذ"));
                }
            }

            ProductList = new(_unitOfWork.Products.GetAll(u => u.QuantityInStock > 0).OrderBy(u => u.Name));
            ListOfSales.Clear();
            TotalAmount = 0;
            _selectedProduct = null;
            IsPieceEnabled = false;
            IsUnitsEnabled = false;

        }

        public void CalcTotalAmount()
        {
            // This method calculates the total amount of the order.
            TotalAmount = ListOfSales.Sum(s => s.TotalPrice);
        }

        public void addProduct(Product product)
        {
            _selectedProduct = ListOfSales.FirstOrDefault(s => s.ProductId == product.ProductId);

            if (_selectedProduct != null)
            {
                if (product.QuantityInStock < _selectedProduct.Quantity)
                    return;

                _selectedProduct.Quantity++;
                Quantity = _selectedProduct.Quantity; // Update the quantity in the UI
                _selectedProduct.TotalPrice = _selectedProduct.Quantity * _selectedProduct.UnitPrice;
                ListOfSales = new(ListOfSales); // Refresh the collection to notify the UI of changes
            }
            else
            {
                _selectedProduct = new SalesCasherModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    UnitPrice = product.UnitPrice,
                    MaxQuantity = product.QuantityInStock, 
                    Quantity = 1,
                    TotalPrice = product.UnitPrice,
                    UnitName = product.UnitName
                };
                Quantity = 1; // Reset quantity to 1 for the new product
                ListOfSales.Add(_selectedProduct);
            }
            CalcTotalAmount();
        }
    }
}
