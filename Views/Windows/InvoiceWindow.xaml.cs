using POS_ModernUI.Models.DTOs;
using POS_ModernUI.Services.Contracts;
using System.Collections.ObjectModel;

namespace POS_ModernUI.Views.Windows;
/// <summary>
/// Interaction logic for InvoiceWindow.xaml
/// </summary>
public partial class InvoiceWindow : Window
{
    public ObservableCollection<SalesCasherModel> SalesCasherModels { get; }
    public string Date { get => DateTime.Now.ToString("hh:mm :: dd-MM-yyyy"); }
    public decimal TotalAmount { get; }

    public InvoiceWindow(ObservableCollection<SalesCasherModel> salesCashers)
    {
        DataContext = this;
        SalesCasherModels = salesCashers;
        TotalAmount = SalesCasherModels.Sum(u => u.TotalPrice);
        InitializeComponent();
    }



    public void PrintDocument(IPrinterService service)
    {
        print.Width = this.Width;
        print.Height = 290 + (SalesCasherModels.Count * 20);
        service.PrintReciept(print);
    }
}
