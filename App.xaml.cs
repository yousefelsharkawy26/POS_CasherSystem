using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using POS_ModernUI.DataAccess.Context;
using POS_ModernUI.DataAccess.UnitOfWork;
using POS_ModernUI.Extensions;
using POS_ModernUI.Models.ViewModels;
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

        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => 
            {
                c.AddJsonFile("appconfig.json", optional: false, reloadOnChange: true);
                c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)!); 
            })
            .ConfigureServices((context, services) =>
            {
                services.AddNavigationViewPageProvider();

                services.AddHostedService<ApplicationHostService>();

                // Main window with navigation
                services.AddApplicationCustomServices()
                        .AddEFCoreConfiguration(context.Configuration)
                        .AddWindows()
                        .AddPages()
                        .AddImageServices();

                


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
            await _host.StartAsync();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
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
