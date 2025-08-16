using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POS_ModernUI.DataAccess.Context;
using POS_ModernUI.DataAccess.UnitOfWork;
using POS_ModernUI.Models.ViewModels;
using POS_ModernUI.Services;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.Services.ImageServices;
using POS_ModernUI.Services.ImageServices.Interface;
using Wpf.Ui;

namespace POS_ModernUI.Extensions;

public static class ApplicationCustomServies
{
    public static IServiceCollection AddApplicationCustomServices(this IServiceCollection services)
    {
        // Theme manipulation
        services.AddSingleton<IThemeService, ThemeService>();

        // TaskBar manipulation
        services.AddSingleton<ITaskBarService, TaskBarService>();

        // Printer Service
        services.AddSingleton<IPrinterService, PrinterService>();

        // Service containing navigation, same as INavigationWindow... but without window
        services.AddSingleton<INavigationService, NavigationService>();

        // Dialog Service
        services.AddSingleton<IDialogService, DialogService>();
        
        services.AddSingleton<IDebtServices, DebtServices>();

        // Registering the UnitOfWork
        services.AddSingleton<IUnitOfWork, UnitOfWork>();

        // Registering the Login User
        services.AddSingleton<CurrentUserModel>();

        return services;
    }

    public static IServiceCollection AddImageServices(this IServiceCollection services)
    {
        // Registering the ImageService
        services.AddSingleton<IImageRemoverService, ImageRemoverService>();
        services.AddSingleton<ImageCompressor>();

        return services;
    }

    public static IServiceCollection AddEFCoreConfiguration(this IServiceCollection services, IConfiguration Configuration)
    {
        // Registering the DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DevConn"))
        );

        return services;
    }
}