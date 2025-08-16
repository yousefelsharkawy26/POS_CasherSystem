using POS_ModernUI.Models;
using POS_ModernUI.Services.Contracts;
using System.Collections.ObjectModel;

namespace POS_ModernUI.ViewModels.Dialogs;
public partial class DebtOrderDetailsViewModel: ObservableObject
{
    private readonly IDebtServices _debtServices;

    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private decimal _totalDebt;
    [ObservableProperty] private decimal _remainingAmount;
    [ObservableProperty] private ObservableCollection<SalesOrder> _invoices = new();

    public DebtOrderDetailsViewModel(IDebtServices debtServices)
    {
        _debtServices = debtServices;
    }

    public async Task LoadCustomerOrdersAsync(int customerId)
    {
        var customerOrders = await _debtServices.GetCustomerOrdersAsync(customerId);
        if (customerOrders != null)
        {
            Invoices.Clear();
            foreach (var order in customerOrders)
            {
                Invoices.Add(order);
            }
        }
    }
}
