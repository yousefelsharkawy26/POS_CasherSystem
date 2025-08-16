using POS_ModernUI.Helpers;
using POS_ModernUI.Models;
using POS_ModernUI.Services.Contracts;
using System.Collections.ObjectModel;

namespace POS_ModernUI.ViewModels.Dialogs;
public partial class NewDebtOrderDialogViewModel: ObservableObject
{
    #region Fields
    private readonly IDebtServices _debtServices;
    #endregion

    #region Props
    [ObservableProperty] private ObservableCollection<Customer> _customerList = new();
    [ObservableProperty] private Customer? _selectedCustomer;
    [ObservableProperty] private bool _isNewCustomer = true;
    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private string? _customerPhone;
    [ObservableProperty] private string? _customerAddress;
    [ObservableProperty] private string? _customerNotes;
    [ObservableProperty] private decimal _paidAmount = 0;
    [ObservableProperty] private SalesOrder _currentOrder = new();
    #endregion

    #region Constructors
    public NewDebtOrderDialogViewModel(IDebtServices debtServices)
    {
        _debtServices = debtServices;

        _ = OnLoaded();
    }
    #endregion

    #region Initializations
    private async Task OnLoaded()
    {
        // Load customers from the database or service
        var customers = await _debtServices.GetAllCustomersAsync();
        if (customers != null)
        {
            CustomerList = new ObservableCollection<Customer>(customers);
        }

        var init = new Customer { Name = "عميل جديد" };

        CustomerList.Insert(0, init);
        SelectedCustomer = init;
    }
    #endregion

    #region Changing
    partial void OnSelectedCustomerChanged(Customer? value)
    {
        if (value == null || value.Id == 0)
        {
            IsNewCustomer = true;
            CustomerName = string.Empty;
            CustomerPhone = null;
            CustomerAddress = null;
            CustomerNotes = null;
        }
        else
        {
            IsNewCustomer = false;
            CustomerName = value.Name;
            CustomerPhone = value.Phone;
            CustomerAddress = value.Address;
            CustomerNotes = value.Notes;
        }
    }
    #endregion

    #region Commands
    public async Task<bool> AddDebt()
    {
        try
        {
            var customer = await GetOrCreateCustomerProcess();
            await CreateDebtOrder(customer, CurrentOrder);

            if (PaidAmount > 0)
                await _debtServices.RecordDebtPaymentAsync(customer.Id, PaidAmount);

            return true;
        }
        catch (Exception ex)
        {
            // Handle error appropriately (log, show message, etc.)
            return false;
        }
    }
    public async Task<bool> OnCloseDialog()
    {
        var msg = new Wpf.Ui.Controls.MessageBox();

        return (await msg.ShowMessageAsync("تنبيه", "هل تريد إلغاء الدين؟", MessageBoxButton.OKCancel) == Wpf.Ui.Controls.MessageBoxResult.Primary);
    }
    #endregion

    private async Task<Customer> GetOrCreateCustomerProcess()
    {
        if (SelectedCustomer != null && SelectedCustomer.Id != 0)
        {
            await Task.CompletedTask;
            return SelectedCustomer;
        }
        else
        {
            var customer = new Customer
            {
                Name = CustomerName,
                Phone = CustomerPhone,
                Address = CustomerAddress,
                Notes = CustomerNotes
            };
            await Task.CompletedTask;
            return customer;
        }
    }
    private async Task CreateDebtOrder(Customer customer, SalesOrder order)
    {
        await _debtServices.AddDebtAsync(customer, order);

        await Task.CompletedTask;
    }
}
