using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Abstractions;
using Microsoft.IdentityModel.Tokens;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.ViewModels.Windows;

namespace POS_ModernUI.Views.Windows;
/// <summary>
/// Interaction logic for LoginWindow.xaml
/// </summary>
public partial class LoginWindow : ILoginNavigationWindow
{
    #region Fields
    IMainNavigationWindow? _navigationWindow;
    IServiceProvider _serviceProvider;
    #endregion

    #region Props
    public LoginViewModel ViewModel { get; }
    #endregion

    #region Constructors
    public LoginWindow(LoginViewModel vm,
                       IServiceProvider serviceProvider)
    {
        ViewModel = vm;
        DataContext = this;
        _serviceProvider = serviceProvider;

        SystemThemeWatcher.Watch(this);

        InitializeComponent();

        txtPassword.Focus();
    }
    #endregion

    #region LoginMethods
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
            if (ViewModel.CanLogin)
                await ViewModel.LoginProcessCommand.ExecuteAsync(null);
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
    #endregion

    #region INavigationWindow methods
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
    #endregion
}
