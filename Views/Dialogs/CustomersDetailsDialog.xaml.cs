using POS_ModernUI.Helpers;
using POS_ModernUI.ViewModels.Dialogs;
using Wpf.Ui.Controls;

namespace POS_ModernUI.Views.Dialogs;
/// <summary>
/// Interaction logic for AddEditCustomerDialog.xaml
/// </summary>
public partial class CustomersDetailsDialog : ContentDialog
{
    public CustomersDetailsDialog()
    {
        InitializeComponent();
    }
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private async void RemoveDebts_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not CustomersDetailsViewModel customersDetails)
            return;

        try
        {
            await customersDetails.RemoveDebtRecordAsync();
        }
        catch
        {
            // Handle error, e.g., show a message to the user
            var msg = new Wpf.Ui.Controls.MessageBox();
            await msg.ShowMessageAsync(Name, "حدث خطأ أثناء حذف العميل. يرجى المحاولة مرة أخرى.");
        }
    }

    private async void RemoveCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not CustomersDetailsViewModel customersDetails)
            return;
        try
        {
            await customersDetails.RemoveCustomerAsync();
            Hide();
        }
        catch
        {
            // Handle error, e.g., show a message to the user
            var msg = new Wpf.Ui.Controls.MessageBox();
            await msg.ShowMessageAsync(Name, "حدث خطأ أثناء حذف العميل. يرجى المحاولة مرة أخرى.");

            Hide();
        }
                        
    }
}
