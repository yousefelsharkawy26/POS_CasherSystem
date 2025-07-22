using POS_ModernUI.Database.Repository.IRepository;
using POS_ModernUI.Models;
using System.Security;
using System.Windows.Controls;

namespace POS_ModernUI.ViewModels.Windows;
public partial class LoginViewModel: ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private CurrentUserModel _currentUserModel;

    [ObservableProperty]
    private string _errorMessage;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private string _applicationTitle = "POS_ModernUI";

    public LoginViewModel(IUnitOfWork unitOfWork, CurrentUserModel currentUserModel)
    {
        _unitOfWork = unitOfWork;
        _currentUserModel = currentUserModel;
    }

    [RelayCommand]
    private void OnLoginProcess()
    {
        var user = _unitOfWork.Users.Get(u => u.Password == Password);

        if (user == null)
        {
            ErrorMessage = "* الباسورد الذى أدخلته غير صحيح";
            return;
        }

        ErrorMessage = string.Empty;
        _currentUserModel.Name = user.Name;
        _currentUserModel.Password = user.Password;
        _currentUserModel.Id = user.Id;

        Name = user.Name;
    }
}
