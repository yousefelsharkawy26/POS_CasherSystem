using POS_ModernUI.DataAccess.UnitOfWork;
using POS_ModernUI.Models.ViewModels;
using Wpf.Ui.Appearance;

namespace POS_ModernUI.ViewModels.Windows;
public partial class LoginViewModel: ObservableObject
{
    #region Fields
    private readonly IUnitOfWork _unitOfWork;
    private CurrentUserModel _currentUserModel;
    #endregion

    #region Props
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _applicationTitle = "POS_ModernUI";
    [ObservableProperty] private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;
    [ObservableProperty] private bool _isDarkTheme = false;
    #endregion

    #region Constructors
    public LoginViewModel(IUnitOfWork unitOfWork, 
                          CurrentUserModel currentUserModel)
    {
        _unitOfWork = unitOfWork;
        _currentUserModel = currentUserModel;
    }
    #endregion

    #region Commands
    [RelayCommand]
    private async Task OnLoginProcess()
    {
        CanLogin = false;
        var user = await _unitOfWork.Users.GetAsync(u => u.Password == Password);

        if (user == null)
        {
            ErrorMessage = "* الباسورد الذى أدخلته غير صحيح";
            return;
        }

        ErrorMessage = string.Empty;
        _currentUserModel.Name = user.Name;
        _currentUserModel.Password = user.Password;
        _currentUserModel.Id = user.Id;
        _currentUserModel.RoleLevel = user.RoleLevel;

        Name = user.Name;
        CanLogin = true;
    }

    [RelayCommand]
    private void OnChangeTheme(string parameter)
    {

        CurrentTheme = IsDarkTheme ? ApplicationTheme.Dark : ApplicationTheme.Light;
        ApplicationThemeManager.Apply(CurrentTheme);
    }

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(LoginProcessCommand))]
    private bool _canLogin = true;
    #endregion
}
