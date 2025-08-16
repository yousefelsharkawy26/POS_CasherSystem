using POS_ModernUI.Services.Contracts;
using POS_ModernUI.ViewModels.Pages;
using POS_ModernUI.Views.Dialogs;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace POS_ModernUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for DebtsView.xaml
    /// </summary>
    public partial class DebtsView : Page
    {
        public DebtsViewModel ViewModel { get; }
        public DebtsView(DebtsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
