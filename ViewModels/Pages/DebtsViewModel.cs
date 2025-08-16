using POS_ModernUI.Models.DTOs;
using POS_ModernUI.Services.Contracts;
using System.Collections.ObjectModel;
using POS_ModernUI.Views.Dialogs;
using POS_ModernUI.ViewModels.Dialogs;
using Wpf.Ui;
using System.Threading.Tasks;

namespace POS_ModernUI.ViewModels.Pages;
public partial class DebtsViewModel: ObservableObject
{
    #region Fields
    private readonly IDebtServices _debtService;
    private readonly IDialogService _dialogService;
    #endregion

    #region Props
    [ObservableProperty] private ObservableCollection<CustomerDebtsDTO> _customerDebts = new();
    [ObservableProperty] private decimal _totalDebts = 0;
    [ObservableProperty] private decimal _remainingAmount = 0;
    [ObservableProperty] private decimal _paidAmount = 0;
    [ObservableProperty] private decimal _persentageDebts = 0;
    [ObservableProperty] private PayDebtDialogViewModel _payDebtDialogViewModel;
    [ObservableProperty] private string _emptyMessage = "لا توجد ديون حالياً";
    [ObservableProperty] private bool _isEmpty = false;
    #endregion

    #region Constructors
    public DebtsViewModel(IDebtServices debtService,
                          IDialogService dialogService)
    {
        _debtService = debtService;
        _dialogService = dialogService;
        _payDebtDialogViewModel = new(_debtService);
        OnLoaded();
    }
    #endregion

    #region Initializations
    private async void OnLoaded()
    {
        // هنا يمكنك إضافة منطق لتحميل البيانات عند تحميل الصفحة
        await LoadCustomerDebts();
    }
    private async Task LoadCustomerDebts()
    {
        // هنا يمكنك إضافة منطق تحميل الديون من مصدر البيانات
        // على سبيل المثال، يمكنك استخدام خدمة لجلب البيانات من قاعدة البيانات
        // محاكاة بيانات الديون
        var customerDebts = await _debtService.GetCustomersAsync();
        if (customerDebts == null || !customerDebts.Any())
        {
            IsEmpty = true; // تعيين الحالة إلى فارغة   
            // إذا لم يكن هناك أي ديون، يمكنك إظهار رسالة أو التعامل مع الحالة كما تريد
            return;
        }

        IsEmpty = false; // تعيين الحالة إلى غير فارغة
        CustomerDebts = new ObservableCollection<CustomerDebtsDTO>(customerDebts);

        CalculateStatistics();
    }
    #endregion

    #region Commands
    [RelayCommand]
    private async Task OnPayDebt(CustomerDebtsDTO dto)
    {
        // هنا يمكنك إضافة منطق دفع الديون
        // مثل فتح نافذة جديدة لإدخال المبلغ المدفوع وتحديث الديون
        PayDebtDialogViewModel = new PayDebtDialogViewModel(_debtService)
        {
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName,
            TotalDebt = dto.TotalDebt,
            RemainingAmount = dto.RemainingAmount,
            OldPayments = await LoadPaymentsAsync(dto.DebtId) // حمّل المدفوعات القديمة
        };

        var msg = new Wpf.Ui.Controls.MessageBox();

        var dialog = new PayDebtDialog
        {
            Title = "دفع الدين",
            DataContext = PayDebtDialogViewModel
        };

        await _dialogService.ShowDialogAsync(dialog);
        await LoadCustomerDebts();
    }

    [RelayCommand]
    private async Task OnViewDebtDetails(CustomerDebtsDTO dto) 
    {
        var viewModel = new DebtOrderDetailsViewModel(_debtService)
        {
            CustomerName = dto.CustomerName,
            TotalDebt = dto.TotalDebt,
            RemainingAmount = dto.RemainingAmount,
        };
        await viewModel.LoadCustomerOrdersAsync(dto.CustomerId);
        // هنا يمكنك إضافة منطق لعرض تفاصيل الدين
        // مثل فتح نافذة جديدة لعرض تفاصيل الدين
        var dialog = new DebtOrderDetailsDialog
        {
            Title = "تفاصيل الدين",
            DataContext = viewModel 
        };
        await _dialogService.ShowDialogAsync(dialog);
    }
    #endregion

    #region Class Helpers
    private void CalculateStatistics()
    {
        decimal totalremaining = 0;
        decimal totaldebts = 0;
        decimal totalpaid = 0;
        decimal persentageOfResentlyPaid = 0;

        foreach (var debt in CustomerDebts)
        {
            totalremaining += debt.RemainingAmount; // استخدام RemainingAmount لحساب المجموع
            totaldebts += debt.TotalDebt;
            totalpaid += debt.PaidAmount;
            persentageOfResentlyPaid += debt.PaidAmount > (debt.TotalDebt / 2) ? 1 : 0; // حساب نسبة الدفعات الأخيرة
        }

        TotalDebts = totaldebts;
        RemainingAmount = totalremaining;
        PaidAmount = totalpaid;
        PersentageDebts = CustomerDebts.Count > 0 ? (persentageOfResentlyPaid / CustomerDebts.Count) * 100 : 0;
    }
    private async Task<ObservableCollection<PaymentHistoryModel>> LoadPaymentsAsync(int DebtId)
    {
        var depts = await _debtService.GetPaymentsByDebtId(DebtId);
        if (depts == null || !depts.Any())
        {
            return new ObservableCollection<PaymentHistoryModel>();
        }

        var payments = await Task.Run(() =>
            new ObservableCollection<PaymentHistoryModel>
            (depts.Select(u => new PaymentHistoryModel
            {
                Amount = u.PaymentAmount,
                Date = u.PaymentDate,
                Note = u.Notes ?? ""
            })));

        return payments;
    }
    #endregion
}
