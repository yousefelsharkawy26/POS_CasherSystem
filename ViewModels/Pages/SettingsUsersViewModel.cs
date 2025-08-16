using POS_ModernUI.DataAccess.UnitOfWork;
using POS_ModernUI.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace POS_ModernUI.ViewModels.Pages;
public partial class SettingsUsersViewModel: ObservableObject
{
    #region Fields
    private readonly IUnitOfWork _unitOfWork;
    #endregion

    #region Properties
    [ObservableProperty] private ObservableCollection<User> _users = new();
    [ObservableProperty] private User? _selectedUser;
    [ObservableProperty] private bool _isEditMode = false;
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _title = "إضافة مستخدم جديد";
    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private int _roleLevel;
    [ObservableProperty] private bool _isCasher = false;
    [ObservableProperty] private bool _isSales = false;
    [ObservableProperty] private bool _isPurchases = false;
    [ObservableProperty] private bool _isProduct = false;
    [ObservableProperty] private bool _isCustomer = false;
    [ObservableProperty] private bool _isDebts = false;
    [ObservableProperty] private bool _isSettings = false;
    [ObservableProperty] private bool _isAll = false;
    #endregion

    #region Constructors
    public SettingsUsersViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _ = LoadUsers();
    }
    #endregion

    #region Commands
    [RelayCommand]
    private async Task OnSaveUser()
    {
        var user = new User
        {
            Name = UserName,
            Password = Password,
        };

        if (IsAll)
        {
            user.RoleLevel = -1; // All permissions
        }
        else
        {
            user.RoleLevel = 0;
            if (IsCasher) user.RoleLevel |= 1; // Casher
            if (IsSales) user.RoleLevel |= 2; // Sales
            if (IsPurchases) user.RoleLevel |= 4; // Purchases
            if (IsProduct) user.RoleLevel |= 8; // Products
            if (IsCustomer) user.RoleLevel |= 16; // Customers
            if (IsDebts) user.RoleLevel |= 32; // Debts
            if (IsSettings) user.RoleLevel |= 64; // Settings
        }

        if (IsEditMode && SelectedUser != null)
        {
            var existingUser = await _unitOfWork.Users.GetAsync(u => u.Id == SelectedUser.Id);
            if (existingUser != null)
            {
                existingUser.Name = user.Name;
                existingUser.Password = user.Password;
                existingUser.RoleLevel = user.RoleLevel;
                // Update user logic here
                await _unitOfWork.Users.UpdateAsync(existingUser);
                
            }
            else
            {
                ErrorMessage = "المستخدم غير موجود.";
            }
        }
        else
        {
            // Add new user logic here
            await _unitOfWork.Users.AddAsync(user);
        }

        await _unitOfWork.SaveAsync();

        ResetForm();
        await LoadUsers();
    }

    [RelayCommand]
    private async Task OnDeleteUser(User user)
    {
        await _unitOfWork.Users.DeleteAsync(user);
        await _unitOfWork.SaveAsync();

        await LoadUsers();
    }

    [RelayCommand]
    private void OnCancel()
    {
        SelectedUser = null;
        IsEditMode = false;
        Title = "إضافة مستخدم جديد";
    }

    [RelayCommand]
    private void OnSelectUser(User user) 
    {
        SelectedUser = user;
        IsEditMode = true;
        Title = "تعديل بيانات المستخدم";
        UserName = user.Name;
        Password = user.Password;
    }
    #endregion

    #region Method Helpers
    private void ResetForm()
    {
        UserName = string.Empty;
        Password = string.Empty;
        RoleLevel = 0;
        IsCasher = false;
        IsSales = false;
        IsPurchases = false;
        IsProduct = false;
        IsCustomer = false;
        IsDebts = false;
        IsSettings = false;
        IsAll = false;
        SelectedUser = null;
        IsEditMode = false;
        Title = "إضافة مستخدم جديد";
        ErrorMessage = string.Empty;
    }
    private async Task LoadUsers()
    {
        // Simulate loading users from a database or service
        Users = new ObservableCollection<User>(await _unitOfWork.Users.GetAllAsync());
    }
    #endregion
}
