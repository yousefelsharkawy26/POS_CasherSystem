using POS_ModernUI.Models;
using POS_ModernUI.Views.Windows;
using System.Collections.ObjectModel;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.Database.Repository.IRepository;

namespace POS_ModernUI.ViewModels.Pages;
public partial class PurchaseManagementViewModel: ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceProvider _serviceProvider;
    private IPurchaseNavigationWindow _navigationWindow;

    [ObservableProperty]
    private ObservableCollection<Supplier> _suppliers;
    [ObservableProperty]
    private ObservableCollection<PurchaseOrder> _purchaseOrders = new();
    [ObservableProperty]
    private ObservableCollection<PurchaseOrderDetail> _purchaseOrdersItems = new();
    [ObservableProperty]
    private string _searchSupplier = string.Empty;

    public PurchaseManagementViewModel(IUnitOfWork unitOfWork,
                                      IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _serviceProvider = serviceProvider;
        Suppliers = new(_unitOfWork.Suppliers.GetAll());
    }

    partial void OnSearchSupplierChanging(string value)
    {
        if (!string.IsNullOrWhiteSpace(value)) 
            Suppliers = new(_unitOfWork.Suppliers.GetAll(u => u.Name.Contains(value)));
        else
            Suppliers = new(_unitOfWork.Suppliers.GetAll());
    }

    [RelayCommand]
    private void OnSelectSupplier(Supplier supplier)
    {
        if (supplier == null)
            return;

        PurchaseOrders = new(_unitOfWork.PurchaseOrders.GetAll(u => u.SupplierId == supplier.SupplierId));
    }

    [RelayCommand]
    private void OnDeleteSupplier(Supplier supplier)
    {
        if (supplier == null)
            return;

        PurchaseOrders.Clear();
        Suppliers.Remove(supplier);
        _unitOfWork.Suppliers.Delete(supplier);
        _unitOfWork.Save();
    }

    [RelayCommand]
    private void OnSelectPurchaseOrder(PurchaseOrder purchaseOrder)
    {
        if (purchaseOrder == null)
            return;

        PurchaseOrdersItems = new(
            _unitOfWork.PurchaseOrderDetails
            .GetAll(u => u.PurchaseOrderId == purchaseOrder.PurchaseOrderId, includeProp:"Product")
        );
    }

    [RelayCommand]
    private void OnDeletePurchaseOrder(PurchaseOrder purchaseOrder)
    {
        _unitOfWork.PurchaseOrders.Delete(purchaseOrder);
        _unitOfWork.Save();

        PurchaseOrders.Remove(purchaseOrder);
    }

    [RelayCommand]
    private async Task OnAddPurchaseOperation()
    {
        _navigationWindow = (
                    _serviceProvider.GetService(typeof(IPurchaseNavigationWindow)) as IPurchaseNavigationWindow
                )!;
        _navigationWindow!.ShowWindow();

        ((AddNewPurchaseWindow)_navigationWindow).Closed += ProductManagementViewModel_Closed;
        await Task.CompletedTask;
    }

    private void ProductManagementViewModel_Closed(object? sender, EventArgs e)
    {
        PurchaseOrders.Clear();
        Suppliers = new(_unitOfWork.Suppliers.GetAll());
    }

    [RelayCommand]
    private void OnDeletePurchaseOrderItem(PurchaseOrderDetail orderItem) 
    {
        if (orderItem == null) return;

        PurchaseOrdersItems.Remove(orderItem);

        _unitOfWork.PurchaseOrderDetails.Delete(orderItem);
        _unitOfWork.Save();
    }
}
