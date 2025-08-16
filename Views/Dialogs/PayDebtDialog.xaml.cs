using Wpf.Ui.Controls;
using POS_ModernUI.ViewModels.Dialogs;

namespace POS_ModernUI.Views.Dialogs;
public partial class PayDebtDialog : ContentDialog
{
    public PayDebtDialog()
    {
        InitializeComponent();
    }

    private async void ConfirmPaymentButton_Click(object sender, RoutedEventArgs e)
    {
        if (await ((PayDebtDialogViewModel)DataContext).OnConfirmPayment())
        {
            this.Hide();
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Hide();
    }
}