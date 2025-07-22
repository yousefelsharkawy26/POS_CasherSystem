using System.Threading.Tasks;
using System.Windows.Controls;
using POS_ModernUI.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace POS_ModernUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for SalesManagementView.xaml
    /// </summary>
    public partial class SalesManagementView : Page
    {
        public SalesManagementViewModel ViewModel { get; }
        public SalesManagementView(SalesManagementViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
            dlgOrderDetails.ButtonClicked += DlgOrderDetails_ButtonClicked;
        }

        private void DlgOrderDetails_ButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            dlgOrderDetails.Visibility = Visibility.Collapsed;
        }

        private void SelectRowDoubleClick_DataGrid(object sender, RoutedEventArgs e)
        {
            dlgOrderDetails.Visibility = Visibility.Visible;
        }
    }
}
