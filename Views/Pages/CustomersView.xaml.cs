using POS_ModernUI.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace POS_ModernUI.Views.Pages;
/// <summary>
/// Interaction logic for CustomersView.xaml
/// </summary>
public partial class CustomersView : INavigableView<CustomersViewModel>
{
    public CustomersViewModel ViewModel { get; }
    public CustomersView(CustomersViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}
