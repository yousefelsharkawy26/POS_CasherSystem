using POS_ModernUI.Models;
using POS_ModernUI.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POS_ModernUI.Views.Windows
{
    /// <summary>
    /// Interaction logic for InvoiceWindow.xaml
    /// </summary>
    public partial class InvoiceWindow : Window
    {
        public ObservableCollection<SalesCasherModel> SalesCasherModels { get; }
        public string Date { get => DateTime.Now.ToString("dd-MM-yyyy hh-mm"); }
        public decimal TotalAmount { get; }
        //public BitmapImage BarCode { get; }

        public InvoiceWindow(ObservableCollection<SalesCasherModel> salesCashers)
        {
            DataContext = this;
            SalesCasherModels = salesCashers;
            TotalAmount = SalesCasherModels.Sum(u => u.TotalPrice);
            //BarCode = barcode;
            InitializeComponent();
        }



        public void PrintDocument(IPrinterService service)
        {
            print.Width = this.Width;
            print.Height = 290 + (SalesCasherModels.Count * 20);
            //ThermalPrinterService.PrintVisual("Microsoft Print to PDF", print, 80, 200);
            service.PrintReciept(print);
        }
    }
}
