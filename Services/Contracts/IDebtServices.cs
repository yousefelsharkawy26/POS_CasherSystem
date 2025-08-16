using POS_ModernUI.Models;
using POS_ModernUI.Models.DTOs;

namespace POS_ModernUI.Services.Contracts;
public interface IDebtServices
{
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<IEnumerable<CustomerDebtsDTO>> GetCustomersAsync();
    Task AddDebtAsync(Customer customer, SalesOrder salesOrder, decimal paidAmount = 0);
    Task<IEnumerable<DebtSale>> GetDebtsByCustomerIdAsync(int customerId);
    Task<IEnumerable<Payment>> GetPaymentsByDebtId(int debtId);
    // تسجيل سداد دين
    Task RecordDebtPaymentAsync(int customerId, decimal amountPaid);
    // حذف سجل دين (مثلاً لو تمت التسوية بالكامل)
    Task RemoveDebtRecordAsync(int customerId);
    Task RemoveCustomer(int customerId);
    Task<Customer> CustomerRegister(Customer customer);
    Task<Customer> CustomerUpdate(int customerId, Customer customer);
    Task<CustomerDetailsDTO> GetCustomerDetailsAsync(int customerId);

    Task<IEnumerable<SalesOrder>> GetCustomerOrdersAsync(int customerId);
}
