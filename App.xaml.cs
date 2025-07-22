﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using POS_ModernUI.Database.Context;
using POS_ModernUI.Database.Repository;
using POS_ModernUI.Database.Repository.IRepository;
using POS_ModernUI.Models;
using POS_ModernUI.Services;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.Services.ImageServices;
using POS_ModernUI.Services.ImageServices.Interface;
using POS_ModernUI.ViewModels.Pages;
using POS_ModernUI.ViewModels.Windows;
using POS_ModernUI.Views.Pages;
using POS_ModernUI.Views.Windows;
using System.IO;
using System.Windows.Threading;
using WebSocketSharp.Server;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;

namespace POS_ModernUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging

        // Add web socket server for barcode scanner
        //private WebSocketServer WSSV;

        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)); })
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<AppDbContext>();
                services.AddSingleton<IUnitOfWork, UnitOfWork>();
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<IImageRemoverService, ImageRemoverService>();
                services.AddSingleton<ImageCompressor>();
                services.AddSingleton<IPQRService>();

                services.AddNavigationViewPageProvider();

                services.AddHostedService<ApplicationHostService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Printer Service
                services.AddSingleton<IPrinterService, PrinterService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Main window with navigation
                services.AddSingleton<ILoginNavigationWindow, LoginWindow>();
                services.AddSingleton<LoginViewModel>();
                services.AddSingleton<IMainNavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddTransient<IProductNavigationWindow, AddEditProductWindow>();
                services.AddTransient<AddEditProductViewModel>();
                services.AddTransient<IPurchaseNavigationWindow, AddNewPurchaseWindow>();
                services.AddTransient<AddNewPurchaseViewModel>();

                services.AddTransient<DashboardPage>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ProductManagementView>();
                services.AddTransient<ProductManagementViewModel>();
                services.AddTransient<PurchaseManagementView>();
                services.AddTransient<PurchaseManagementViewModel>();
                services.AddTransient<SalesManagementView>();
                services.AddTransient<SalesManagementViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();

                services.AddSingleton<CurrentUserModel>();
            }).Build();

        /// <summary>
        /// Gets services.
        /// </summary>
        public static IServiceProvider Services
        {
            get { return _host.Services; }
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            //await Task.Run(() =>
            //{
            //    WSSV = new WebSocketServer("ws://0.0.0.0:5050");
            //    WSSV.AddWebSocketService<BarcodeTCPSocket>("/barcode");
            //    WSSV.Start();
            //});

            await _host.StartAsync();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            //await Task.Run(() =>
            //{
            //    WSSV.Stop();
            //});

            await _host.StopAsync();
            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
            e.Handled = true;
            
        }
    }
}
