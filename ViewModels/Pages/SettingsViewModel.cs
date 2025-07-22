using Microsoft.IdentityModel.Tokens;
using POS_ModernUI.Database.Repository.IRepository;
using POS_ModernUI.Helpers;
using POS_ModernUI.Models;
using POS_ModernUI.Services;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media.Imaging;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace POS_ModernUI.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private readonly IUnitOfWork _unitOfWork;
        private CurrentUserModel _currentUser;
        private IPQRService _qrService;
        public SettingsViewModel(IUnitOfWork unitOfWork,
                                CurrentUserModel currentUser,
                                IPQRService iPQRService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;

            UserName = _currentUser.Name;
            Password = _currentUser.Password;
            _qrService = iPQRService;
        }

        private bool _isInitialized = false;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        [ObservableProperty]
        private bool _isDarkTheme = false;

        [ObservableProperty]
        private string _userName = string.Empty;
        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private BitmapImage _ipQRImage; 


        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();

            if (CurrentTheme == ApplicationTheme.Dark)
                IsDarkTheme = true;
            else
                IsDarkTheme = false;

            AppVersion = $"UiDesktopApp - {GetAssemblyVersion()} By Elsharkawy";

            _isInitialized = true;

            //IpQRImage = _qrService.GenerateQRCode(GetLocalIPAddress(), 5050);
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            if (IsDarkTheme)
            {
                CurrentTheme = ApplicationTheme.Dark;
                ApplicationThemeManager.Apply(ApplicationTheme.Dark);
            }
            else
            {
                CurrentTheme = ApplicationTheme.Light;
                ApplicationThemeManager.Apply(ApplicationTheme.Light);
            }
        }

        [RelayCommand]
        private void OnChangeUserNameAndPassword()
        {
            if (UserName.IsNullOrEmpty() || Password.IsNullOrEmpty())
                return;

            var currentUser = _unitOfWork.Users.Get(u => u.Id == _currentUser.Id);
            
            currentUser.Name = UserName;
            currentUser.Password = Password;

            _unitOfWork.Users.Update(currentUser);
            _unitOfWork.Save();

            Wpf.Ui.Controls.MessageBox msg = new();
            msg.ShowMessage("تم تغيير اسم المستخدم وكلمة المرور بنجاح", "عملية ناجحة");
        }
    
        private string? GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            var ip = host.AddressList.FirstOrDefault(u => u.AddressFamily == AddressFamily.InterNetwork)?.ToString();

            return ip;
        }
    }
}
