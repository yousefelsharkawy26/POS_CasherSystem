using Microsoft.IdentityModel.Tokens;
using POS_ModernUI.DataAccess.UnitOfWork;
using POS_ModernUI.Helpers;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.ViewModels.Pages;
using POS_ModernUI.Views.Windows;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace POS_ModernUI.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardViewModel>
    {
        public DashboardViewModel ViewModel { get; }
        DispatcherTimer _timer;
        private IPrinterService _printer;
        IUnitOfWork _unitOfWork;
        IDialogService _dialogService;

        public DashboardPage(DashboardViewModel viewModel,
                             IPrinterService printer,
                             IUnitOfWork unitOfWork,
                             IDialogService dialogService)
        {
            ViewModel = viewModel;
            DataContext = this;
            _printer = printer;
            _unitOfWork = unitOfWork;
            InitializeComponent();

            this.Loaded += onLoaded;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                txtBarcode.Focus();
                Keyboard.Focus(txtBarcode);
            }), System.Windows.Threading.DispatcherPriority.Input);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _timer.Tick += (s, e) =>
            {
                if (!txtBarcode.IsFocused)
                {
                    txtBarcode.Focus();
                    Keyboard.Focus(txtBarcode);
                }

            };
            _timer.Start();

            this.Unloaded += (s, e) => _timer.Stop();
            _dialogService = dialogService;
            //_dialogService.SetContentPresenter(ContentPresenter);
        }

        private void onLoaded(object sender, RoutedEventArgs e)
        {
            txtBarcode.Focus();
            Keyboard.Focus(txtBarcode);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            txtBarcode.Focus();
            Keyboard.Focus(txtBarcode);
        }
        private async void txtBarCode_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.TextBox textBox && e.Key == System.Windows.Input.Key.Enter)
            {
                // Trigger the search functionality when Enter is pressed
                if (string.IsNullOrWhiteSpace(textBox.Text))
                    return;

                //var productUnit = _unitOfWork.ProductUnits.Get(pu => pu.ProductBarCode == textBox.Text, "Product");
                var product = await _unitOfWork.Products.GetAsync(p => p.UnitShares.Any(u => u.ProductBarCode == textBox.Text), "UnitShares.Unit");

                if (product == null)
                {
                    // Optionally, you can show a message if the product is not found
                    Wpf.Ui.Controls.MessageBox msg = new();
                    await msg.ShowMessageAsync("المنتج غير موجود", "خطأ");
                    textBox.Clear();
                    return;
                }

                var productUnit = product.UnitShares.FirstOrDefault(u => u.ProductBarCode == textBox.Text);

                await ViewModel.AddProduct(product, productUnit);

                textBox.Clear();
            }
            txtBarcode.Focus();
            Keyboard.Focus(txtBarcode);
        }
        private void Workersdata_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {

            ViewModel.CalculateTotalAmount();
            txtBarcode.Focus();
            Keyboard.Focus(txtBarcode);
        }
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Optionally, you can show a success message or navigate to another page.
            if (!ViewModel.ListOfSales.IsNullOrEmpty())
            {
                Wpf.Ui.Controls.MessageBox msg = new();
                if (msg.ShowMessage("هل تريد طباعة الطلب", "طباعة الطلب", System.Windows.MessageBoxButton.OKCancel) == Wpf.Ui.Controls.MessageBoxResult.Primary)
                {
                    // Implement the logic to print the order here.
                    InvoiceWindow invoice = new InvoiceWindow(ViewModel.ListOfSales);
                    invoice.PrintDocument(_printer);
                }

                await ViewModel.OrderCompletedCommand.ExecuteAsync(null);
            }

            txtBarcode.Focus();
            Keyboard.Focus(txtBarcode);
        }
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // Optionally, you can show a success message or navigate to another page.
            if (!ViewModel.ListOfSales.IsNullOrEmpty())
            {
                var lst = ViewModel.ListOfSales;

                await ViewModel.OrderCompletedWithDebt();
                
                _timer.Stop();
                await ViewModel.ShowDebtDialogAsync();
                _timer.Start();

                Wpf.Ui.Controls.MessageBox msg = new();
                if (await msg.ShowMessageAsync("هل تريد طباعة الطلب", "طباعة الطلب", System.Windows.MessageBoxButton.OKCancel) == Wpf.Ui.Controls.MessageBoxResult.Primary)
                {
                    // Implement the logic to print the order here.
                    InvoiceWindow invoice = new InvoiceWindow(lst);
                    invoice.PrintDocument(_printer);
                }
            }

            txtBarcode.Focus();
            Keyboard.Focus(txtBarcode);
        }
        private void Page_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            txtBarcode.Focus();
            Keyboard.Focus(txtBarcode);
        }
        private void DlgOrderDetails_ButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _timer.Start();
        }
        private async Task ShowDialogAsync()
        {
            await Task.CompletedTask;
        }
    }
}
