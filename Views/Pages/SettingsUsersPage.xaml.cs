using POS_ModernUI.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace POS_ModernUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for SettingsUsersPage.xaml
    /// </summary>
    public partial class SettingsUsersPage : INavigableView<SettingsUsersViewModel>
    {
        public SettingsUsersViewModel ViewModel { get; }
        public SettingsUsersPage(SettingsUsersViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

    }
}
