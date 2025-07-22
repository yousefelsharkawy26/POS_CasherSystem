using POS_ModernUI.Database.Repository.IRepository;
using POS_ModernUI.Models;
using System.Collections.ObjectModel;

namespace POS_ModernUI.ViewModels.Pages;
public partial class SalesManagementViewModel: ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty]
    private SalesOrder _selectedSalesOrder;
    [ObservableProperty]
    private ObservableCollection<SalesOrder> _salesOrders;
    [ObservableProperty]
    private ObservableCollection<SalesOrderDetail> _salesOrderItems = new();
    [ObservableProperty]
    private ProductSalesModel _productSalesModels = new();
    [ObservableProperty]
    private string _totalSalesCost;
    [ObservableProperty]
    private string _searchSalesOrder;
    public SalesManagementViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        SalesOrders = new(_unitOfWork.SalesOrders.GetAll());
        TotalSalesCost = SalesOrders.Sum(x => x.TotalAmount).ToString("c");

        UpdateChartElements();
    }

    partial void OnSearchSalesOrderChanging(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            SalesOrders = new(_unitOfWork.SalesOrders.GetAll(u => u.SalesOrderBarcode.Contains(value)));
        }
        else
        {
            SalesOrders = new(_unitOfWork.SalesOrders.GetAll());
        }
        TotalSalesCost = SalesOrders.Sum(x => x.TotalAmount).ToString("c");
        UpdateChartElements();
    }
    void UpdateChartElements(bool isToday = false)
    {
        var salesItems = _unitOfWork.SalesOrderDetails.GetAll(includeProp: "Product,SalesOrder").ToList();
        var products = _unitOfWork.Products.GetAll().ToList();

        IEnumerable<IGrouping<string, SalesOrderDetail>> result = new List<IGrouping<string, SalesOrderDetail>>(); 
        if (!isToday)
        {
            result = (from p in products
                      join s in salesItems
                      on p.Name equals s.Product.Name
                      group s by s.Product.Name into g
                      select g);
        }
        else
        {
            result = (from p in products
                      join s in salesItems
                      on p.Name equals s.Product.Name
                      where s.SalesOrder.Date == DateOnly.FromDateTime(DateTime.Now)
                      group s by s.Product.Name into g
                      select g);
        }

        ProductSalesModels.ProductNames = new string[result.Count()];
        ProductSalesModels.Quantities = new();

        int i = 0;
        foreach (var item in result)
        {
            ProductSalesModels.ProductNames[i] = item.Key;
            ProductSalesModels.Quantities.Add(item.Sum(u => u.Quantity));
            i++;
        }

        OnPropertyChanged(nameof(ProductSalesModels));
        OnPropertyChanged(nameof(ProductSalesModels.ProductNames));
        OnPropertyChanged(nameof(ProductSalesModels.Quantities));
    }

    [RelayCommand]
    private void OnSelectSalesOrder(SalesOrder salesOrder)
    {
        if (salesOrder == null) return;

        SalesOrderItems = new(
            _unitOfWork.SalesOrderDetails
            .GetAll(x => x.SalesOrderId == salesOrder.SalesOrderId, includeProp: "Product")
        );
    }

    [RelayCommand]
    private void OnDeleteSalesOrder(SalesOrder salesOrder)
    {
        if (salesOrder == null) return;
        var salesOrderItems = _unitOfWork.SalesOrderDetails
            .GetAll(x => x.SalesOrderId == salesOrder.SalesOrderId).ToList();

        foreach (var item in salesOrderItems)
            OnDeleteSalesOrderItem(item);

        _unitOfWork.SalesOrders.Delete(salesOrder);
        _unitOfWork.Save();

        SalesOrders.Remove(salesOrder);
        SalesOrderItems.Clear();
    }

    [RelayCommand]
    private void OnDeleteSalesOrderItem(SalesOrderDetail salesOrderItem)
    {
        if (salesOrderItem == null) return;

        _unitOfWork.SalesOrderDetails.Delete(salesOrderItem);
        
        SalesOrderItems.Remove(salesOrderItem);

        var product = _unitOfWork.Products.Get(u => u.ProductId == salesOrderItem.ProductId);

        product.QuantityInStock += salesOrderItem.Quantity;

        _unitOfWork.Products.Update(product);

        _unitOfWork.Save();
    }

    [RelayCommand]
    private void OnSelectAllSales()
    {
        SalesOrders = new(_unitOfWork.SalesOrders.GetAll());
        TotalSalesCost = SalesOrders.Sum(x => x.TotalAmount).ToString("c");
        UpdateChartElements();
    }

    [RelayCommand]
    private void OnSelectTodaySales()
    {
        var dt = DateTime.Now;
        SalesOrders = new(_unitOfWork.SalesOrders
            .GetAll(u => u.Date.Year == dt.Year && u.Date.Month == dt.Month && u.Date.Day == dt.Day)
        );

        TotalSalesCost = SalesOrders.Sum(x => x.TotalAmount).ToString("c");
        UpdateChartElements(true);
    }
}
