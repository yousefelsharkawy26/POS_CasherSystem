using POS_ModernUI.Models;
using POS_ModernUI.Views.Windows;
using System.Collections.ObjectModel;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.DataAccess.UnitOfWork;
using System.Threading.Tasks;

namespace POS_ModernUI.ViewModels.Pages;
public partial class ProductManagementViewModel: ObservableObject
{
    #region Fields
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceProvider _serviceProvider;
    private IProductNavigationWindow? _navigationWindow;
    #endregion

    #region Props
    [ObservableProperty] private string _searchProducts = string.Empty;
    [ObservableProperty] private int _expiredProductsCount;
    [ObservableProperty] private int _outOfStockProductsCount;
    [ObservableProperty] private int _totalProductsCount;
    [ObservableProperty] private decimal _totalPrice = 0;
    [ObservableProperty] private ObservableCollection<Product> _ListOfProducts = new();
    #endregion

    #region Constructors
    public ProductManagementViewModel(IUnitOfWork unitOfWork,
                                      IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _serviceProvider = serviceProvider;
        _ = LoadProductsAsync();
    }
    #endregion

    #region Initializations
    private async Task LoadProductsAsync()
    {
        ListOfProducts = new(await _unitOfWork.Products.GetAllAsync(includeProp: "UnitShares.Unit"));
        CalculateStatistics();
    }
    private void CalculateStatistics()
    {
        TotalProductsCount = ListOfProducts.Count;
        ExpiredProductsCount = ListOfProducts.Count(p => p.QuantityInStock < 10 && p.QuantityInStock > 0);
        OutOfStockProductsCount = ListOfProducts.Count(p => p.QuantityInStock == 0);
        TotalPrice = ListOfProducts.Sum(u =>
        {
            var productUnit = u.UnitShares.FirstOrDefault(p => p.QuantityPerParent != null);
            if (productUnit == null)
                productUnit = u.UnitShares.FirstOrDefault();

            if (productUnit == null)
                productUnit = new();

            return productUnit.UnitPrice * u.QuantityInStock;
        });
    }
    #endregion

    #region Commands
    [RelayCommand]
    private async Task OnAddNewProduct()
    {
        _navigationWindow = (
                    _serviceProvider.GetService(typeof(IProductNavigationWindow)) as IProductNavigationWindow
                )!;
        _navigationWindow!.ShowWindow();

        ((AddEditProductWindow)_navigationWindow).Closed += ProductManagementViewModel_Closed;
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnUpdateProduct(Product product)
    {
        _navigationWindow = (
                    _serviceProvider.GetService(typeof(IProductNavigationWindow)) as IProductNavigationWindow
                )!;
        _navigationWindow!.ShowWindow(product);

        ((AddEditProductWindow)_navigationWindow).Closing += ProductManagementViewModel_Closed;

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnRemoveProduct(Product product)
    {
        if (product == null)
            return;

        ListOfProducts.Remove(product);
        await _unitOfWork.Products.DeleteAsync(product);
        await _unitOfWork.SaveAsync();

        // Change State Of Header
        TotalProductsCount = ListOfProducts.Count;
        ExpiredProductsCount = ListOfProducts.Count(p => p.QuantityInStock < 10 && p.QuantityInStock > 0);
        OutOfStockProductsCount = ListOfProducts.Count(p => p.QuantityInStock == 0);
        TotalPrice = ListOfProducts.Sum(u =>
        {
            var productUnit = u.UnitShares.FirstOrDefault(p => p.QuantityPerParent != null);

            if (productUnit == null)
                productUnit = u.UnitShares.FirstOrDefault();

            if (productUnit == null)
                productUnit = new();

            return productUnit.UnitPrice * u.QuantityInStock;
        });
    }
    #endregion

    #region Class Helpers
    partial void OnSearchProductsChanging(string value)
    {
        _ = SearchOnProduct(value);
    }
    private async Task SearchOnProduct(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            ListOfProducts = new(
                await _unitOfWork.Products.GetAllAsync(p => p.Name.Contains(value) || p.UnitShares.Any(u => u.ProductBarCode == value), "UnitShares.Unit")
            );
        else
            ListOfProducts = new(await _unitOfWork.Products.GetAllAsync(includeProp: "UnitShares.Unit"));

        CalculateStatistics();
    }
    private async void ProductManagementViewModel_Closed(object? sender, EventArgs e)
    {
        // Refresh the product list after adding a new product
        await LoadProductsAsync();
    }
    #endregion

}
