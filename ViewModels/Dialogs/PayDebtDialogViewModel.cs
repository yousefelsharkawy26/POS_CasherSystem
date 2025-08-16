using POS_ModernUI.Helpers;
using POS_ModernUI.Models.DTOs;
using POS_ModernUI.Services.Contracts;
using System.Collections.ObjectModel;

namespace POS_ModernUI.ViewModels.Dialogs;
public partial class PayDebtDialogViewModel : ObservableObject
{
    #region Fields
    private readonly IDebtServices _debtServices;
    #endregion

    #region Props
    [ObservableProperty] public int _customerId;
    [ObservableProperty] public string _customerName = "";
    [ObservableProperty] public string _errorMessage = "";
    [ObservableProperty] public decimal _totalDebt;
    [ObservableProperty] public decimal _remainingAmount;
    [ObservableProperty] public ObservableCollection<PaymentHistoryModel> _oldPayments = new();
    [ObservableProperty] public decimal _newPaymentAmount;
    #endregion

    #region Constructors
    public PayDebtDialogViewModel(IDebtServices debtServices)
    {
        _debtServices = debtServices;
    }
    #endregion

    #region ClassActions
    public async Task<bool> OnConfirmPayment()
    {
        // تحقق من صحة المبلغ
        if (NewPaymentAmount <= 0 || NewPaymentAmount > RemainingAmount)
        {
            // أظهر رسالة خطأ
            ErrorMessage = "يجب ان يكون الرقم اصغر من او يساوى المبلغ المتبقى واكبر من الصفر";
            return false;
        }
        // أضف الدفع الجديد للقاعدة أو القائمة
        // أغلق النافذة أو أرسل النتيجة
        await _debtServices.RecordDebtPaymentAsync(CustomerId, NewPaymentAmount);

        // رسالة الاغلاق الناجحة
        var msg = new Wpf.Ui.Controls.MessageBox();
        

        return await msg.ShowMessageAsync("تمت العملية بنجاح", "نجح") == Wpf.Ui.Controls.MessageBoxResult.Primary;
    }
    #endregion
}

