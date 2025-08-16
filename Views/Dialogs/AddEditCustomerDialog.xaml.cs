using Wpf.Ui.Controls;

namespace POS_ModernUI.Views.Dialogs;
/// <summary>
/// Interaction logic for AddEditCustomerDialog.xaml
/// </summary>
public partial class AddEditCustomerDialog : ContentDialog
{
    public AddEditCustomerDialog()
    {
        InitializeComponent();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ViewModels.Dialogs.AddEditCustomerViewModel viewModel)
            return;

        if (await viewModel.SaveCustomer())
        {
            Hide();
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
