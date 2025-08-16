using POS_ModernUI.Models;
using POS_ModernUI.Models.DTOs;
using Microsoft.Extensions.Logging;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.DataAccess.UnitOfWork;

namespace POS_ModernUI.Services;
public class DebtServices : IDebtServices
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DebtServices> _logger;
    public DebtServices(IUnitOfWork unitOfWork, 
                        ILogger<DebtServices> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task AddDebtAsync(Customer customer, SalesOrder salesOrder, decimal paidAmount = 0)
    {
        var customerTask = await CustomerRegister(customer);

        var debtSale = await AddNewDebt(customerTask, salesOrder);

        debtSale.TotalAmount = salesOrder.TotalAmount;
        debtSale.PaidAmount = paidAmount;
        try
        {
            await _unitOfWork.DebtSales.UpdateAsync(debtSale);
            await _unitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            // هنا يمكنك تسجيل الخطأ أو التعامل معه كما تراه مناسبًا
            _logger.LogError(ex, "Error adding debt for customer {CustomerId} with sales order {SalesOrderId}", 
                customerTask.Id, salesOrder.SalesOrderId);
        }
    }
    public async Task<IEnumerable<DebtSale>> GetDebtsByCustomerIdAsync(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("Invalid customer ID", nameof(customerId));

        return await _unitOfWork.DebtSales.GetAllAsync(d => d.CustomerId == customerId, "Payments,Invoice,Customer");
    }
    public async Task RecordDebtPaymentAsync(int customerId, decimal amountPaid)
    {
        // تحقق من وجود سجل الدين المرتبط بالدفع
        var debtSales = await _unitOfWork.DebtSales.GetAllAsync(u => u.CustomerId == customerId && (u.TotalAmount - u.PaidAmount) > 0);

        if (debtSales == null || !debtSales.Any())
        {
            _logger.LogWarning("DebtSale with Id {customerId} not found for payment", customerId);
            throw new InvalidOperationException($"DebtSale with Id {customerId} not found.");
        }
        // تحديث المبلغ المدفوع في سجل الدين

        foreach (var debtSale in debtSales)
        {
            
            // إضافة الدفع إلى سجل المدفوعات
            var payment = new Payment
            {
                CreditSaleId = debtSale.Id,
                PaymentAmount = 0, // سيتم تحديثه لاحقًا
            };


            if (debtSale.RemainingAmount > amountPaid)
            {
                debtSale.PaidAmount += amountPaid;
                payment.PaymentAmount = amountPaid;
                payment.Notes = $"تم الدفع الجزئي لـ {amountPaid} في {DateTime.Now}";

                amountPaid = 0; // تم دفع المبلغ بالكامل
            }

            else if (debtSale.RemainingAmount <= amountPaid)
            {
                payment.PaymentAmount = debtSale.RemainingAmount;
                payment.Notes = $"تم الدفع الكامل لـ {debtSale.RemainingAmount} في {DateTime.Now}";

                amountPaid -= payment.PaymentAmount;

                debtSale.PaidAmount = debtSale.TotalAmount; // تحديث المبلغ المدفوع بالكامل
            }

            await _unitOfWork.DebtSales.UpdateAsync(debtSale);
            await _unitOfWork.Payments.AddAsync(payment);


            if (amountPaid <= 0)
                break; // إذا تم دفع المبلغ بالكامل، نخرج من الحلقة
        }

        await _unitOfWork.SaveAsync();
    }
    public async Task RemoveDebtRecordAsync(int customerId)
    {
        var debtSales = await _unitOfWork.DebtSales.GetAllAsync(u => u.CustomerId == customerId);

        if (debtSales == null || !debtSales.Any())
        {
            _logger.LogWarning("No debt records found for customer {CustomerId}", customerId);
            throw new InvalidOperationException($"No debt records found for customer {customerId}.");
        }
        foreach (var debtSale in debtSales)
        {
            await _unitOfWork.DebtSales.DeleteAsync(debtSale);
        }
        // Commit Changes
        await _unitOfWork.SaveAsync();
    }
    public async Task<IEnumerable<CustomerDebtsDTO>> GetCustomersAsync()
    {
        var customers = await _unitOfWork.Cutomers.GetAllAsync(u => u.DebtSales.Any(d => d.TotalAmount > d.PaidAmount), "DebtSales.Payments");

        if (!customers.Any())
        {
            _logger.LogInformation("No customers found with debts.");
            return await Task.FromResult(Enumerable.Empty<CustomerDebtsDTO>());
        }

        var customerDebts = await Task.Run(() => customers.Select(c => new CustomerDebtsDTO
        {
            CustomerId = c.Id,
            CustomerName = c.Name,
            TotalDebt = c.DebtSales.Where(d => d.RemainingAmount > 0).Sum(d => d.TotalAmount),
            PaidAmount = c.DebtSales.Where(d => d.RemainingAmount > 0).Sum(d => d.PaidAmount),
            Date = c.DebtSales.FirstOrDefault(d => d.RemainingAmount > 0)?.Date ?? DateTime.Now,
            DebtId = c.DebtSales.FirstOrDefault(d => d.RemainingAmount > 0)?.Id ?? 0,

        }).ToList());

        return customerDebts;
    }
    public async Task<IEnumerable<Payment>> GetPaymentsByDebtId(int debtId)
    {
        var payments = await _unitOfWork.Payments.GetAllAsync(p => p.CreditSaleId == debtId);

        return payments ?? Enumerable.Empty<Payment>();
    }
    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _unitOfWork.Cutomers.GetAllAsync(includeProp: "DebtSales");
    }
    public async Task<CustomerDetailsDTO> GetCustomerDetailsAsync(int customerId)
    {
        var customer = await _unitOfWork.Cutomers.GetAsync(c => c.Id == customerId, includeProp: "DebtSales.Payments");
        
        if (customer == null)
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found.", customerId);
            return await Task.FromResult<CustomerDetailsDTO>(null);
        }

        var customerDetails = await Task.Run( () => new CustomerDetailsDTO
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerPhone = customer.Phone ?? string.Empty,
            CustomerAddress = customer.Address ?? string.Empty,
            CustomerNotes = customer.Notes ?? string.Empty,
            LastPaymentDate = customer.DebtSales
                .Where(d => d.Payments.Any())
                .SelectMany(d => d.Payments)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault()?.PaymentDate ?? DateTime.MinValue,
            TotalDebt = customer.DebtSales.Sum(d => d.TotalAmount),
            TotalPaid = customer.DebtSales.Sum(d => d.PaidAmount),
            RemainingDebt = customer.DebtSales.Sum(d => d.TotalAmount - d.PaidAmount)
        });

        foreach (var debtSale in customer.DebtSales)
        {
            var debtSaleDto = new DebtSaleDTO
            {
                Id = debtSale.Id,
                CustomerId = debtSale.CustomerId,
                SaleDate = debtSale.Date,
                TotalAmount = debtSale.TotalAmount,
                PaidAmount = debtSale.PaidAmount,
                SalesOrder = debtSale.Invoice, // Assuming Invoice is the SalesOrder
                Payments = debtSale.Payments.ToList() ?? new List<Payment>(),
            };
            customerDetails.DebtSales.Add(debtSaleDto);
        }

        return customerDetails;
    }

    public async Task<IEnumerable<SalesOrder>> GetCustomerOrdersAsync(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("Invalid customer ID", nameof(customerId));

        return (await _unitOfWork
            .DebtSales
            .GetAllAsync(o => o.CustomerId == customerId && o.TotalAmount > o.PaidAmount, includeProp: "Invoice.SalesOrderItems"))
            .Select(u => u.Invoice);
    }

    #region Class Helpers
    private async Task<bool> IsExistingCustomer(Customer customer)
    {
        return (await _unitOfWork.Cutomers.GetAllAsync()).Any(c => c.Id == customer.Id);
    }
    public async Task<Customer> CustomerRegister(Customer customer)
    {
        if (!await IsExistingCustomer(customer))
        {
            await _unitOfWork.Cutomers.AddAsync(customer);
            await _unitOfWork.SaveAsync();
        }

        return customer;
    }
    public async Task<Customer> CustomerUpdate(int customerId ,Customer customer)
    {
        var existingCustomer = await _unitOfWork.Cutomers.GetAsync(u => u.Id == customerId);

        if (existingCustomer != null)
        {
            existingCustomer.Name = customer.Name;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Address = customer.Address;
            existingCustomer.Notes = customer.Notes;

            await _unitOfWork.Cutomers.UpdateAsync(existingCustomer);
            await _unitOfWork.SaveAsync();
        }

        return customer;
    }
    private async Task<DebtSale> AddNewDebt(Customer customer, SalesOrder order)
    {
        var debtSale = new DebtSale
        {
            CustomerId = customer.Id,
            InvoiceId = order.SalesOrderId,
            TotalAmount = 0, // سيتم تحديثه لاحقًا
            Date = DateTime.Now,
            PaidAmount = 0
        };

        await _unitOfWork.DebtSales.AddAsync(debtSale);
        await _unitOfWork.SaveAsync();

        return debtSale;
    }
    public async Task RemoveCustomer(int customerId)
    {
        var customer = await _unitOfWork.Cutomers.GetAsync(u => u.Id == customerId);

        if (customer != null)
        {
            await _unitOfWork.Cutomers.DeleteAsync(customer);
            await _unitOfWork.SaveAsync();
        }
    }
    #endregion
}
