using POS_ModernUI.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace POS_ModernUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for PurchaseManagementView.xaml
    /// </summary>
    public partial class PurchaseManagementView : Page
    {
        public PurchaseManagementViewModel ViewModel { get; }
        public PurchaseManagementView(PurchaseManagementViewModel viewModel)
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
