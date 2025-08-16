using Microsoft.IdentityModel.Tokens;
using POS_ModernUI.Helpers;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.ViewModels.Windows;
using System.Windows.Controls;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace POS_ModernUI.Views.Windows
{
    /// <summary>
    /// Interaction logic for AddEditProductWindow.xaml
    /// </summary>
    public partial class AddNewPurchaseWindow : IPurchasesNavigationWindow
    {
        #region Props
        public AddNewPurchaseViewModel ViewModel { get; }
        #endregion

        #region Constructors
        public AddNewPurchaseWindow(AddNewPurchaseViewModel vm)
        {
            ViewModel = vm;
            DataContext = this;

            SystemThemeWatcher.Watch(this);

            InitializeComponent();
        }
        #endregion

        #region Navigation Props
        public void ShowWindow() => Show();
        public void CloseWindow() => Close();
        public INavigationView GetNavigation() => throw new NotImplementedException();
        public bool Navigate(Type pageType) => throw new NotImplementedException();
        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => throw new NotImplementedException();
        public void SetServiceProvider(IServiceProvider serviceProvider) => throw new NotImplementedException();
        #endregion

        #region WindowActions
        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;

            if (cb?.SelectedValue == null) return;

            if (int.TryParse(cb?.SelectedValue.ToString(), out int value) && value == 0)
            {
                txtSupplierName.Visibility = Visibility.Visible;
                txtSupplierPhone.Visibility = Visibility.Visible;
            }
            else
            {
                txtSupplierName.Visibility = Visibility.Collapsed;
                txtSupplierPhone.Visibility = Visibility.Collapsed;
            }
        }
        private async void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var txt = sender as Wpf.Ui.Controls.TextBox;

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // add logic Barcode reader here
                await ViewModel.SearchProductByBarcodeAsync(txt?.Text!);
            }
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.NewPurchaseOrders.IsNullOrEmpty())
                return;

            ViewModel.SavePurchasesCommand.Execute(null);

            var msg = new Wpf.Ui.Controls.MessageBox();
            if (await msg.ShowMessageAsync("هل تريد اضافة فاتورة جديدة", "تم حفظ الفاتورة بنجاح", System.Windows.MessageBoxButton.OKCancel)
                != Wpf.Ui.Controls.MessageBoxResult.Primary) 
            {
                this.Close();
                return;
            }

            ViewModel.RestartView();
        }
        #endregion
    }
}
