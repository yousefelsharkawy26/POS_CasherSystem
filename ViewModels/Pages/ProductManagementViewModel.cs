using POS_ModernUI.Models;
using POS_ModernUI.Views.Windows;
using System.Collections.ObjectModel;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.Database.Repository.IRepository;

namespace POS_ModernUI.ViewModels.Pages;
public partial class ProductManagementViewModel: ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceProvider _serviceProvider;
    private IProductNavigationWindow _navigationWindow;

    [ObservableProperty]
    private string _searchProducts = string.Empty;

    [ObservableProperty]
    private int _expiredProductsCount;
    [ObservableProperty]
    private int _outOfStockProductsCount;
    [ObservableProperty]
    private int _totalProductsCount;


    [ObservableProperty]
    private ObservableCollection<Product> _ListOfProducts;

    public ProductManagementViewModel(IUnitOfWork unitOfWork,
                                      IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _serviceProvider = serviceProvider;
        ListOfProducts = new(_unitOfWork.Products.GetAll());
        TotalProductsCount = ListOfProducts.Count;
        ExpiredProductsCount = ListOfProducts.Count(p => p.QuantityInStock < 10 && p.QuantityInStock > 0);
        OutOfStockProductsCount = ListOfProducts.Count(p => p.QuantityInStock == 0);
    }

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

    private void ProductManagementViewModel_Closed(object? sender, EventArgs e)
    {
        // Refresh the product list after adding a new product
        ListOfProducts = new(_unitOfWork.Products.GetAll());
        TotalProductsCount = ListOfProducts.Count;
        ExpiredProductsCount = ListOfProducts.Count(p => p.QuantityInStock < 10 && p.QuantityInStock > 0);
        OutOfStockProductsCount = ListOfProducts.Count(p => p.QuantityInStock == 0);

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
    private void OnRemoveProduct(Product product)
    {
        if (product == null)
            return;

        ListOfProducts.Remove(product);
        _unitOfWork.Products.Delete(product);
        _unitOfWork.Save();

        // Change State Of Header
        TotalProductsCount = ListOfProducts.Count;
        ExpiredProductsCount = ListOfProducts.Count(p => p.QuantityInStock < 10 && p.QuantityInStock > 0);
        OutOfStockProductsCount = ListOfProducts.Count(p => p.QuantityInStock == 0);
    }
    partial void OnSearchProductsChanging(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            ListOfProducts = new(
                _unitOfWork.Products.GetAll(p => p.Name.Contains(value) || p.Barcode == value)
            );
        else
            ListOfProducts = new(_unitOfWork.Products.GetAll());
    }
}
