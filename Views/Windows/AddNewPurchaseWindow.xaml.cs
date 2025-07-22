using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Abstractions;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.ViewModels.Windows;
using System.Windows.Controls;
using POS_ModernUI.Helpers;
using Microsoft.IdentityModel.Tokens;

namespace POS_ModernUI.Views.Windows
{
    /// <summary>
    /// Interaction logic for AddEditProductWindow.xaml
    /// </summary>
    public partial class AddNewPurchaseWindow : IPurchaseNavigationWindow
    {
        public AddNewPurchaseViewModel ViewModel { get; }
        public AddNewPurchaseWindow(AddNewPurchaseViewModel vm)
        {
            ViewModel = vm;
            DataContext = this;

            SystemThemeWatcher.Watch(this);

            InitializeComponent();
        }

        public void ShowWindow() => Show();
        public void CloseWindow() => Close();

        public INavigationView GetNavigation()
        {
            throw new NotImplementedException();
        }
        public bool Navigate(Type pageType)
        {
            throw new NotImplementedException();
        }
        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider)
        {
            throw new NotImplementedException();
        }
        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;

            if (int.TryParse(cb.SelectedValue.ToString(), out int value) && value == 0)
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
        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var txt = sender as Wpf.Ui.Controls.TextBox;

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // add logic Barcode reader here
                ViewModel.GetProductByBarCode(txt.Text);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.NewPurchaseOrders.IsNullOrEmpty())
                return;

            ViewModel.OnSavePurchases();

            var msg = new Wpf.Ui.Controls.MessageBox();
            if (msg.ShowMessage("هل تريد اضافة فاتورة جديدة", "تم حفظ الفاتورة بنجاح", System.Windows.MessageBoxButton.OKCancel)
                != Wpf.Ui.Controls.MessageBoxResult.Primary) 
            {
                this.Close();
                return;
            }

            ViewModel.RestartView();
        }
    }
}
