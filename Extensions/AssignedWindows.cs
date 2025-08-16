using Microsoft.Extensions.DependencyInjection;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.ViewModels.Windows;
using POS_ModernUI.Views.Windows;

namespace POS_ModernUI.Extensions;
internal static class AssignedWindows
{
    public static IServiceCollection AddWindows(this IServiceCollection services)
    {
        // Registering the LoginNavigationWindow
        services.AddSingleton<ILoginNavigationWindow, LoginWindow>();
        services.AddSingleton<LoginViewModel>();
        // Registering the MainNavigationWindow
        services.AddSingleton<IMainNavigationWindow, MainWindow>();
        services.AddSingleton<MainWindowViewModel>();
        // Registering the AddEditProductWindow
        services.AddTransient<IProductNavigationWindow, AddEditProductWindow>();
        services.AddTransient<AddEditProductViewModel>();
        // Registering the AddNewPurchaseWindow
        services.AddTransient<IPurchasesNavigationWindow, AddNewPurchaseWindow>();
        services.AddTransient<AddNewPurchaseViewModel>();
        return services;
    }
}
