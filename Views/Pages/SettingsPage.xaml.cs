using POS_ModernUI.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace POS_ModernUI.Views.Pages
{
    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.StartCountdownCommand.Execute(this);
        }
    }
}
