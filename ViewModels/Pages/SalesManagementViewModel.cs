using POS_ModernUI.DataAccess.UnitOfWork;
using POS_ModernUI.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace POS_ModernUI.ViewModels.Pages;
public partial class SalesManagementViewModel: ObservableObject
{
    #region Fields
    private readonly IUnitOfWork _unitOfWork;
    #endregion

    #region Props
    [ObservableProperty] private SalesOrder? _selectedSalesOrder;
    [ObservableProperty] private ObservableCollection<SalesOrder> _salesOrders = new();
    [ObservableProperty] private ObservableCollection<SalesOrderDetail> _salesOrderItems = new();
    [ObservableProperty] private ProductSalesModel _productSalesModels = new();
    [ObservableProperty] private decimal _totalSalesCost = 0;
    #endregion
    
    #region Constructors
    public SalesManagementViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _ = InitializeAsync();
    }
    #endregion

    #region Initializations
    private async Task InitializeAsync()
    {
        SalesOrders = new(await _unitOfWork.SalesOrders.GetAllAsync());
        TotalSalesCost = SalesOrders.Sum(x => x.TotalAmount);

        await UpdateChartElements();
    }
    private async Task UpdateChartElements(bool isToday = false)
    {
        var salesItems = (await _unitOfWork.SalesOrderDetails.GetAllAsync(includeProp: "Product,SalesOrder")).ToList();
        var products = (await _unitOfWork.Products.GetAllAsync()).ToList();

        IEnumerable<IGrouping<string, SalesOrderDetail>> result = new List<IGrouping<string, SalesOrderDetail>>(); 
        if (!isToday)
        {
            result = (from p in products
                      join s in salesItems
                      on p.Name equals s.Product?.Name
                      group s by s.Product?.Name into g
                      select g);
        }
        else
        {
            result = (from p in products
                      join s in salesItems
                      on p.Name equals s.Product?.Name
                      where s.SalesOrder?.Date == DateOnly.FromDateTime(DateTime.Now)
                      group s by s.Product?.Name into g
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
    #endregion

    #region Commands
    [RelayCommand]
    private async Task OnSelectSalesOrder(SalesOrder salesOrder)
    {
        if (salesOrder == null) return;

        SalesOrderItems = new(
            await _unitOfWork.SalesOrderDetails
            .GetAllAsync(x => x.SalesOrderId == salesOrder.SalesOrderId, includeProp: "Product")
        );
    }

    [RelayCommand]
    private async Task OnDeleteSalesOrder(SalesOrder salesOrder)
    {
        if (salesOrder == null) return;
        var salesOrderItems = (await _unitOfWork.SalesOrderDetails
            .GetAllAsync(x => x.SalesOrderId == salesOrder.SalesOrderId)).ToList();

        foreach (var item in salesOrderItems)
            await OnDeleteSalesOrderItem(item);

        await _unitOfWork.SalesOrders.DeleteAsync(salesOrder);
        await _unitOfWork.SaveAsync();

        SalesOrders.Remove(salesOrder);
        SalesOrderItems.Clear();
        TotalSalesCost = SalesOrders.Sum(x => x.TotalAmount);
    }

    [RelayCommand]
    private async Task OnDeleteSalesOrderItem(SalesOrderDetail salesOrderItem)
    {
        if (salesOrderItem == null) return;

        await _unitOfWork.SalesOrderDetails.DeleteAsync(salesOrderItem);
        
        SalesOrderItems.Remove(salesOrderItem);

        var product = await _unitOfWork.Products.GetAsync(u => u.ProductId == salesOrderItem.ProductId);

        product.QuantityInStock += salesOrderItem.Quantity;

        await _unitOfWork.Products.UpdateAsync(product);

        await _unitOfWork.SaveAsync();
        TotalSalesCost = SalesOrders.Sum(x => x.TotalAmount);
    }

    [RelayCommand]
    private async Task OnSelectAllSales()
    {
        await InitializeAsync();
    }

    [RelayCommand]
    private async Task OnSelectTodaySales()
    {
        var dt = DateTime.Now;
        SalesOrders = new(await _unitOfWork.SalesOrders
            .GetAllAsync(u => u.Date.Year == dt.Year && u.Date.Month == dt.Month && u.Date.Day == dt.Day)
        );

        TotalSalesCost = SalesOrders.Sum(x => x.TotalAmount);
        await UpdateChartElements(true);
    }
    #endregion
}
