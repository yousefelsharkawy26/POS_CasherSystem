using POS_ModernUI.Models;
using POS_ModernUI.Services.Contracts;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace POS_ModernUI.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "POS_ModernUI";

        [ObservableProperty]
        private INotificationService _notifications;

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "الكاشير",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(Views.Pages.DashboardPage)
            },
            new NavigationViewItem()
            {
                Content = "ادارة المبيعات",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DataHistogram24 },
                TargetPageType = typeof(Views.Pages.SalesManagementView)
            },
            new NavigationViewItem()
            {
                Content = "ادارة المشترايات",
                Icon = new SymbolIcon { Symbol = SymbolRegular.BuildingRetailMoney24 },
                TargetPageType = typeof(Views.Pages.PurchaseManagementView)
            },
            new NavigationViewItem()
            {
                Content = "ادارة المنتجات",
                Icon = new SymbolIcon { Symbol = SymbolRegular.ProductionCheckmark20 },
                TargetPageType = typeof(Views.Pages.ProductManagementView)
            }
        };

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };

        public MainWindowViewModel(INotificationService notifications)
        {
            Notifications = notifications;
        }

        [RelayCommand]
        private void OnNavegateNotifications(Notification notification)
        {
            // Navigate to the notification page or perform an action based on the notification
            // Update Notification To Read
            Notifications.SetRead(notification);
            OnPropertyChanged(nameof(Notifications));
            OnPropertyChanged(nameof(Notifications.Notifications));

        }

        [RelayCommand]
        private void OnRemoveNotification(Notification notification)
        {
            Notifications.RemoveNotification(notification);
            OnPropertyChanged(nameof(Notifications));
            OnPropertyChanged(nameof(Notifications.Notifications));
        }
    }
}
