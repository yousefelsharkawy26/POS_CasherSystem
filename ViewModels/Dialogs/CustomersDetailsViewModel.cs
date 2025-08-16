using POS_ModernUI.Models;
using POS_ModernUI.Models.DTOs;
using POS_ModernUI.Services.Contracts;
using System.Collections.ObjectModel;

namespace POS_ModernUI.ViewModels.Dialogs
{
    public partial class CustomersDetailsViewModel : ObservableObject
    {
        #region Fields
        private IDebtServices debtService;
        #endregion

        #region Props
        [ObservableProperty] private string _emptyMessage = "لا توجد تفاصيل للعميل";
        [ObservableProperty] private bool _isNotEmpty = true;
        [ObservableProperty] private bool _isEmpty = true;
        [ObservableProperty] private bool _isLoading = false;
        [ObservableProperty] private CustomerDetailsDTO _customerDetails = new();
        public int CustomerId = 0; // معرف العميل الذي سيتم تحميل تفاصيله
        #endregion

        public CustomersDetailsViewModel(IDebtServices debtService, int customerId)
        {
            this.debtService = debtService;
            CustomerId = customerId;
            FactoryMethod();
        }

        private async void FactoryMethod()
        {
            await LoadCustomerDetailsAsync();
        }

        private async Task LoadCustomerDetailsAsync()
        {
            IsLoading = true;
            CustomerDetails = await debtService.GetCustomerDetailsAsync(CustomerId);

            if (CustomerDetails == null || CustomerDetails.DebtSales.Count == 0)
            {
                IsNotEmpty = false; // تعيين الحالة إلى فارغة
                IsEmpty = true; // تعيين الحالة إلى فارغة
            }
            else
            {
                IsNotEmpty = true; // تعيين الحالة إلى غير فارغة
                IsEmpty = false; // تعيين الحالة إلى فارغة
            }
            IsLoading = false;
        }

        public async Task RemoveCustomerAsync()
        {
            if (CustomerId <= 0)
            {
                return; // أو يمكنك إظهار رسالة خطأ
            }
            // استدعاء خدمة حذف العميل
            await debtService.RemoveCustomer(CustomerId);
        }

        public async Task RemoveDebtRecordAsync()
        {
            if (CustomerId <= 0)
            {
                return; // أو يمكنك إظهار رسالة خطأ
            }
            // استدعاء خدمة حذف سجل الدين
            await debtService.RemoveDebtRecordAsync(CustomerId);
            await LoadCustomerDetailsAsync();
        }
    }
}