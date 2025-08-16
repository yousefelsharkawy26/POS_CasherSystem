using POS_ModernUI.Models;
using POS_ModernUI.Services.Contracts;

namespace POS_ModernUI.ViewModels.Dialogs;
public partial class AddEditCustomerViewModel: ObservableObject
{
    #region Fields
    private readonly IDebtServices _debtService;
    #endregion

    #region Props
    [ObservableProperty] private int _customerId;
    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private string _customerPhone = string.Empty;
    [ObservableProperty] private string _customerAddress = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _dialogNotes = string.Empty;
    [ObservableProperty] private bool _isEditMode = false;
    #endregion
    public AddEditCustomerViewModel(IDebtServices debtService)
    {
        _debtService = debtService;
        // Constructor logic if needed
    }

    public async Task<bool> SaveCustomer()
    {
        // هنا يمكنك إضافة منطق حفظ العميل
        // على سبيل المثال، يمكنك استخدام خدمة لحفظ البيانات في قاعدة البيانات
        if (string.IsNullOrWhiteSpace(CustomerName) || string.IsNullOrWhiteSpace(CustomerPhone))
        {
            ErrorMessage = "الاسم ورقم الهاتف مطلوبان.";
            return false;
        }
        var customer = new Customer
        {
            Name = CustomerName,
            Phone = CustomerPhone,
            Address = CustomerAddress,
            Notes = DialogNotes
        };

        // إذا كان في وضع التعديل، قم بتحديث العميل الحالي
        if (IsEditMode)
        {
            await _debtService.CustomerUpdate(CustomerId, customer);   
        }
        else
        {
            // إنشاء عميل جديد
            // هنا يمكنك إضافة منطق الإنشاء
            await _debtService.CustomerRegister(customer);
        }

        return true;
    }
}
