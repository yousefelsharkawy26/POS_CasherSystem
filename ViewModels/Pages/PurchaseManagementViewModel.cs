using POS_ModernUI.DataAccess.UnitOfWork;
using POS_ModernUI.Models;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.Views.Windows;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Wpf.Ui;

namespace POS_ModernUI.ViewModels.Pages;
public partial class PurchaseManagementViewModel: ObservableObject
{
    #region Fields
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceProvider _serviceProvider;
    private IPurchasesNavigationWindow? _navigationWindow;
    #endregion

    #region Props
    [ObservableProperty] private ObservableCollection<Supplier> _suppliers = new();
    [ObservableProperty] private ObservableCollection<PurchaseOrder> _purchaseOrders = new();
    [ObservableProperty] private ObservableCollection<PurchaseOrderDetail> _purchaseOrdersItems = new();
    [ObservableProperty] private string _searchSupplier = string.Empty;
    #endregion

    #region Constructors
    public PurchaseManagementViewModel(IUnitOfWork unitOfWork,
                                      IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _serviceProvider = serviceProvider;
        _ = InitializeAsync();
    }
    #endregion

    #region Initializations
    private async Task InitializeAsync()
    {
        Suppliers = new(await _unitOfWork.Suppliers.GetAllAsync());
    }
    #endregion

    #region Class Changing
    partial void OnSearchSupplierChanging(string value)
    {
        _ = SearchSupplierHelper(value);
    }
    private async Task SearchSupplierHelper(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            Suppliers = new(await _unitOfWork.Suppliers.GetAllAsync(u => u.Name.Contains(value)));
        else
            Suppliers = new(await _unitOfWork.Suppliers.GetAllAsync());
    }
    #endregion

    #region Commands
    [RelayCommand]
    private async Task OnSelectSupplier(Supplier supplier)
    {
        if (supplier == null)
            return;

        PurchaseOrders = new(await _unitOfWork.PurchaseOrders.GetAllAsync(u => u.SupplierId == supplier.SupplierId));
    }

    [RelayCommand]
    private async Task OnDeleteSupplier(Supplier supplier)
    {
        if (supplier == null)
            return;

        PurchaseOrders.Clear();
        Suppliers.Remove(supplier);
        await _unitOfWork.Suppliers.DeleteAsync(supplier);
        await _unitOfWork.SaveAsync();
    }

    [RelayCommand]
    private async Task OnSelectPurchaseOrder(PurchaseOrder purchaseOrder)
    {
        if (purchaseOrder == null)
            return;

        PurchaseOrdersItems = new(
            await _unitOfWork.PurchaseOrderDetails
            .GetAllAsync(u => u.PurchaseOrderId == purchaseOrder.PurchaseOrderId, includeProp:"Product")
        );
    }

    [RelayCommand]
    private async Task OnDeletePurchaseOrder(PurchaseOrder purchaseOrder)
    {
        await _unitOfWork.PurchaseOrders.DeleteAsync(purchaseOrder);
        await _unitOfWork.SaveAsync();

        PurchaseOrders.Remove(purchaseOrder);
    }

    [RelayCommand]
    private async Task OnAddPurchaseOperation()
    {
        _navigationWindow = (
                    _serviceProvider.GetService(typeof(IPurchasesNavigationWindow)) as IPurchasesNavigationWindow
                )!;
        _navigationWindow!.ShowWindow();

        ((AddNewPurchaseWindow)_navigationWindow).Closed += ProductManagementViewModel_Closed;
        await Task.CompletedTask;
    }

    private async void ProductManagementViewModel_Closed(object? sender, EventArgs e)
    {
        PurchaseOrders.Clear();
        Suppliers = new(await _unitOfWork.Suppliers.GetAllAsync());
    }

    [RelayCommand]
    private async Task OnDeletePurchaseOrderItem(PurchaseOrderDetail orderItem) 
    {
        if (orderItem == null) return;

        PurchaseOrdersItems.Remove(orderItem);

        await _unitOfWork.PurchaseOrderDetails.DeleteAsync(orderItem);
        await _unitOfWork.SaveAsync();
    }
    #endregion
}
