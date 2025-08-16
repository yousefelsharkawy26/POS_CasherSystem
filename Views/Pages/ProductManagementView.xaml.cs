using POS_ModernUI.ViewModels.Pages;
using System.Windows.Controls;

namespace POS_ModernUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for ProductManagementView.xaml
    /// </summary>
    public partial class ProductManagementView : Page
    {
        public ProductManagementViewModel ViewModel { get; }  
        public ProductManagementView(ProductManagementViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
