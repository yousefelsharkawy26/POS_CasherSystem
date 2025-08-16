using Wpf.Ui.Controls;
using POS_ModernUI.ViewModels.Dialogs;

namespace POS_ModernUI.Views.Dialogs;
/// <summary>
/// Interaction logic for NewDebtOrderDialog.xaml
/// </summary>
public partial class NewDebtOrderDialog : ContentDialog
{
    public NewDebtOrderDialog()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        this.Hide();
    }
    private async void AddDebtClick(object sender, RoutedEventArgs e)
    {
        if ((await ((NewDebtOrderDialogViewModel)DataContext).AddDebt()) == true)
        {
            this.Hide();
        }
    }
}
