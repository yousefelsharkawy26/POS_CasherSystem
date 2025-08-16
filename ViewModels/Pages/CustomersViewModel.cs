using POS_ModernUI.Helpers;
using POS_ModernUI.Models;
using POS_ModernUI.Services.Contracts;
using System.Collections.ObjectModel;

namespace POS_ModernUI.ViewModels.Pages;
public partial class CustomersViewModel : ObservableObject
{
    #region Fields
    private readonly IDebtServices _debtService;
    private readonly IDialogService _dialogService;
    #endregion

    #region Props
    [ObservableProperty] private string _emptyMessage = "لا توجد عملاء حالياً";
    [ObservableProperty] private bool _isEmpty = true;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private string _appVersion = string.Empty;
    #endregion

    #region Constructors
    public CustomersViewModel(IDebtServices debtService, 
                              IDialogService dialogService)
    {
        _debtService = debtService;
        _dialogService = dialogService;
        _ = OnLoaded();
    }
    #endregion

    #region Initializations
    private async Task OnLoaded()
    {
        IsLoading = true;
        await LoadCustomers();
        IsLoading = false;
    }
    private async Task LoadCustomers()
    {
        // هنا يمكنك إضافة منطق تحميل العملاء من مصدر البيانات
        // على سبيل المثال، يمكنك استخدام خدمة لجلب البيانات من قاعدة البيانات
        // محاكاة بيانات العملاء
        Customers = new ObservableCollection<Customer>(await _debtService.GetAllCustomersAsync());
        if (Customers == null || !Customers.Any())
        {
            IsEmpty = true; // تعيين الحالة إلى فارغة   
            return;
        }

        IsEmpty = false; // تعيين الحالة إلى غير فارغة
    }
    #endregion

    #region Class Changed
    partial void OnSearchTextChanged(string value)
    {
        _ = SearchCustomersAsync();
    }
    #endregion

    #region Commands
    [RelayCommand]
    private async Task OnAddOrUpdateCustomer(Customer? customer)
    {
        var viewModel = new Dialogs.AddEditCustomerViewModel(_debtService)
        {
            CustomerAddress = customer?.Address ?? string.Empty,
            CustomerName = customer?.Name ?? string.Empty,
            CustomerPhone = customer?.Phone ?? string.Empty,
            DialogNotes = customer?.Notes ?? string.Empty,
            CustomerId = customer?.Id ?? 0
        };

        var dialog = new Views.Dialogs.AddEditCustomerDialog();
        dialog.IsFooterVisible = false; // إظهار تذييل الحوار

        if (customer == null)
        {
            viewModel.IsEditMode = false; // وضع إضافة عميل جديد
            dialog.Title = "إضافة عميل جديد";
        }
        else
        {
            viewModel.IsEditMode = true; // وضع تعديل عميل موجود
            dialog.Title = "تعديل عميل";
        }

        dialog.DataContext = viewModel;
        
        await _dialogService.ShowDialogAsync(dialog);

        await LoadCustomers();
    } 
    [RelayCommand]
    private async Task OnDeleteCustomer(Customer customer)
    {
        if (customer == null) return;
        // هنا يمكنك إضافة منطق حذف العميل
        // على سبيل المثال، يمكنك استخدام خدمة لحذف البيانات من قاعدة البيانات
        // محاكاة حذف العميل
        bool hasDebts = customer.DebtSales.Any(u => u.TotalAmount > u.PaidAmount);

        var msg = new Wpf.Ui.Controls.MessageBox();
        if (hasDebts)
            if (await msg.ShowMessageAsync(
                "لا يمكن حذف العميل لأنه لديه ديون غير مسددة.\n حذف على اى حال", "تحذير",
                MessageBoxButton.OKCancel) != Wpf.Ui.Controls.MessageBoxResult.Primary)
                return;
        
        await _debtService.RemoveCustomer(customer.Id);

        Customers.Remove(customer);
        await LoadCustomers();
    }
    [RelayCommand]
    private async Task OnShowCustomersDetails(Customer customer)
    {
        var viewModel = new Dialogs.CustomersDetailsViewModel(_debtService, customer.Id);

        var dialog = new Views.Dialogs.CustomersDetailsDialog
        {
            IsFooterVisible = false, // إظهار تذييل الحوار
            DataContext = viewModel
        };

        await _dialogService.ShowDialogAsync(dialog);
        // بعد إغلاق الحوار، يمكنك تحديث قائمة العملاء إذا لزم الأمر
        await LoadCustomers();
    }
    #endregion

    #region Class Helpers
    private async Task SearchCustomersAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadCustomers();
            return;
        }
        IsLoading = true;
        var allCustomers = await _debtService.GetAllCustomersAsync();
        var filteredCustomers = allCustomers
            .Where(c => c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || (c.Phone != null && c.Phone.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        Customers = new ObservableCollection<Customer>(filteredCustomers);
        IsEmpty = !Customers.Any();
        IsLoading = false;
    }
    #endregion
}
