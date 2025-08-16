using POS_ModernUI.Helpers;
using POS_ModernUI.Models.ViewModels;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace POS_ModernUI.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private CurrentUserModel _currentUserModel;

        public MainWindowViewModel(CurrentUserModel currentUserModel)
        {
            _currentUserModel = currentUserModel;

            if (PermissionHelper.HasPermission(_currentUserModel.RoleLevel, PermissionType.Casher))
                _menuItems.Add(new NavigationViewItem()
                {
                    Content = "الكاشير",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                    TargetPageType = typeof(Views.Pages.DashboardPage)
                });

            if (PermissionHelper.HasPermission(_currentUserModel.RoleLevel, PermissionType.Sales))
                _menuItems.Add(new NavigationViewItem()
                {
                    Content = "ادارة المبيعات",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.DataHistogram24 },
                    TargetPageType = typeof(Views.Pages.SalesManagementView)
                });

            if (PermissionHelper.HasPermission(_currentUserModel.RoleLevel, PermissionType.Purchases))
                _menuItems.Add(new NavigationViewItem()
                {
                    Content = "ادارة المشترايات",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.BuildingRetailMoney24 },
                    TargetPageType = typeof(Views.Pages.PurchaseManagementView)
                });

            if (PermissionHelper.HasPermission(_currentUserModel.RoleLevel, PermissionType.Products))
                _menuItems.Add(new NavigationViewItem()
                {
                    Content = "ادارة المنتجات",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.ProductionCheckmark20 },
                    TargetPageType = typeof(Views.Pages.ProductManagementView)
                });

            if (PermissionHelper.HasPermission(_currentUserModel.RoleLevel, PermissionType.Customers))
                _menuItems.Add(new NavigationViewItem()
                {
                    Content = "قائمة العملاء",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.Person24 },
                    TargetPageType = typeof(Views.Pages.CustomersView)
                });

            if (PermissionHelper.HasPermission(_currentUserModel.RoleLevel, PermissionType.Debts))
                _menuItems.Add(new NavigationViewItem()
                {
                    Content = "قائمة الديون",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.CreditCardClock20 },
                    TargetPageType = typeof(Views.Pages.DebtsView)
                });

            if (PermissionHelper.HasPermission(_currentUserModel.RoleLevel, PermissionType.Debts))
            {
                _footerMenuItems.Add(new NavigationViewItem()
                {
                    Content = "إعدادات المستخدم",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.AppsSettings20 },
                    TargetPageType = typeof(Views.Pages.SettingsUsersPage)
                });

                _footerMenuItems.Add(new NavigationViewItem()
                {
                    Content = "الإعدادات",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                    TargetPageType = typeof(Views.Pages.SettingsPage)
                });
            }
        }


        [ObservableProperty]
        private string _applicationTitle = Application.ResourceAssembly.GetName().Name!;


        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new();

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new();

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };
    }
}
