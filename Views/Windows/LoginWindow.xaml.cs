using Microsoft.IdentityModel.Tokens;
using POS_ModernUI.Models;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.ViewModels.Windows;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace POS_ModernUI.Views.Windows
{
    /// <summary>
    /// Interaction logic for AddEditProductWindow.xaml
    /// </summary>
    public partial class LoginWindow : ILoginNavigationWindow
    {
        Product _product;
        IMainNavigationWindow _navigationWindow;
        IServiceProvider _serviceProvider;
        public LoginViewModel ViewModel { get; }
        public LoginWindow(LoginViewModel vm,
                           IMainNavigationWindow navigationWindow,
                           IServiceProvider serviceProvider)
        {
            ViewModel = vm;
            DataContext = this;
            _navigationWindow = navigationWindow;
            _serviceProvider = serviceProvider;

            SystemThemeWatcher.Watch(this);

            InitializeComponent();

            txtPassword.Focus();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (txtUserName.Text.IsNullOrEmpty())
                return;

            

            await StartAsync(CancellationToken.None);
            this.Close();
        }

        private async void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;

            if (txtUserName.Text.IsNullOrEmpty())
            {
                ViewModel.LoginProcessCommand.Execute(null);
                return;
            }

            await StartAsync(CancellationToken.None);
            this.Close();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await HandleActivationAsync();
        }

        private async Task HandleActivationAsync()
        {
            _navigationWindow = (
                _serviceProvider.GetService(typeof(IMainNavigationWindow)) as IMainNavigationWindow
            )!;
            _navigationWindow!.ShowWindow();

            _navigationWindow.Navigate(typeof(Views.Pages.DashboardPage));

            await Task.CompletedTask;
        }


        public void ShowWindow() => Show();

        public void CloseWindow() => Application.Current.Shutdown();
        
        public INavigationView GetNavigation()
        {
            throw new NotImplementedException();
        }

        public bool Navigate(Type pageType)
        {
            throw new NotImplementedException();
        }

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider)
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        private void TitleBar_CloseClicked(TitleBar sender, RoutedEventArgs args)
        {
            Application.Current.Shutdown();
        }

        
    }
}
