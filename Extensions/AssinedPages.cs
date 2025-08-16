using Microsoft.Extensions.DependencyInjection;
using POS_ModernUI.ViewModels.Pages;
using POS_ModernUI.Views.Pages;

namespace POS_ModernUI.Extensions;

static class AssinedPages
{
    public static IServiceCollection AddPages(this IServiceCollection services)
    {
        // Registering the HomePage
        services.AddTransient<DashboardPage>();
        services.AddTransient<DashboardViewModel>();
        // Registering the ProductsPage
        services.AddTransient<ProductManagementView>();
        services.AddTransient<ProductManagementViewModel>();
        // Registering the PurchasesPage
        services.AddTransient<PurchaseManagementView>();
        services.AddTransient<PurchaseManagementViewModel>();
        // Registering the SalesPage
        services.AddTransient<SalesManagementView>();
        services.AddTransient<SalesManagementViewModel>();
        // Registering the DebtsPage
        services.AddTransient<DebtsView>();
        services.AddTransient<DebtsViewModel>();
        // Registering the CustomersPage
        services.AddTransient<CustomersView>();
        services.AddTransient<CustomersViewModel>();
        // Registering the SettingsPage
        services.AddSingleton<SettingsPage>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<SettingsUsersPage>();
        services.AddSingleton<SettingsUsersViewModel>();

        return services;
    }
}
