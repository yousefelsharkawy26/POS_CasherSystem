using POS_ModernUI.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;
using POS_ModernUI.Helpers;
using POS_ModernUI.Services.Contracts;
using System.Windows.Controls;
using POS_ModernUI.Views.Windows;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using POS_ModernUI.Services;
using POS_ModernUI.Database.Repository.IRepository;

namespace POS_ModernUI.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardViewModel>
    {
        public DashboardViewModel ViewModel { get; }
        private IPrinterService _printer;
        private StringBuilder barcodeBuilder = new();
        private DateTime lastKeyTime = DateTime.Now;
        private string lastBarcode = string.Empty;
        private DateTime lastScanTime = DateTime.MinValue;
        IPQRService _qrService;
        IUnitOfWork _unitOfWork;

        public DashboardPage(DashboardViewModel viewModel,
                             IPrinterService printer,
                             IPQRService qrService,
                             IUnitOfWork unitOfWork)
        {
            ViewModel = viewModel;
            DataContext = this;
            _printer = printer;
            _qrService = qrService;
            _unitOfWork = unitOfWork;
            InitializeComponent();
            this.Loaded += (sender, e) => { txtBarcode.Focus(); };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            txtBarcode.Focus();
        }
        private void txtBarCode_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.TextBox textBox && e.Key == System.Windows.Input.Key.Enter)
            {
                // Trigger the search functionality when Enter is pressed
                var product = _unitOfWork.Products.Get(p => p.Barcode == textBox.Text);
                if (product == null)
                {
                    // Optionally, you can show a message if the product is not found
                    Wpf.Ui.Controls.MessageBox msg = new();
                    msg.ShowMessage("المنتج غير موجود", "خطأ");
                    return;
                }

                ViewModel.addProduct(product);

                textBox.Clear();
            }
            txtBarcode.Focus();
        }
        private void Workersdata_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {

            ViewModel.CalcTotalAmount();
            this.Focus();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Optionally, you can show a success message or navigate to another page.
            if (ViewModel.ListOfSales.IsNullOrEmpty())
                return;

            Wpf.Ui.Controls.MessageBox msg = new();
            if (msg.ShowMessage("هل تريد طباعة الطلب", "طباعة الطلب", System.Windows.MessageBoxButton.OKCancel) == Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                string newBarcode = $"{DateTime.Now:yyyyMMddHHmmss}-{ViewModel.ListOfSales.Count}";
                //var salesorderbarcode = _qrService.GenerateBarcode(newBarcode);
                ViewModel.SalesOrderBarcode = newBarcode;
                // Implement the logic to print the order here.
                InvoiceWindow invoice = new InvoiceWindow(ViewModel.ListOfSales);
                invoice.PrintDocument(_printer);
            }

            ViewModel.OrderCompletedCommand.Execute(null);

            this.Focus();
        }
        private void Page_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
            //if (e.Key == System.Windows.Input.Key.Enter)
            //{
            //    string barcode = barcodeBuilder.ToString().Trim();
            //    barcodeBuilder.Clear();

            //    if (!string.IsNullOrWhiteSpace(barcode)) 
            //    {
            //        //var now = DateTime.Now;
            //        //if (barcode == lastBarcode && (now - lastScanTime).TotalMilliseconds < 1000)
            //        //    return; // تجاهل التكرار

            //        //lastBarcode = barcode;
            //        //lastScanTime = now;

            //        //var product = ViewModel.ProductList.FirstOrDefault(p => p.Barcode == barcode);
            //        //if (product == null)
            //        //{
            //        //    // Optionally, you can show a message if the product is not found
            //        //    Wpf.Ui.Controls.MessageBox msg = new();
            //        //    msg.ShowMessage("المنتج غير موجود", "خطأ");
            //        //    return;
            //        //}

            //        //ViewModel.addProduct(product);
            //    }
            //}
            txtBarcode.Focus();
        }
        private void Page_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - lastKeyTime;

            if (elapsed.TotalMilliseconds > 100)
                barcodeBuilder.Clear();

            barcodeBuilder.Append(e.Text);
            lastKeyTime = DateTime.Now;
        }
    }
}
