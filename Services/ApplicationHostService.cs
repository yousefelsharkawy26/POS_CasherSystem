using Wpf.Ui;
using System.Diagnostics;
using POS_ModernUI.Views.Windows;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;
using POS_ModernUI.Services.Contracts;

namespace POS_ModernUI.Services
{
    /// <summary>
    /// Managed host of the application.
    /// </summary>
    public class ApplicationHostService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private Mutex _mutex;
        private ILoginNavigationWindow _navigationWindow;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_SHOWMAXIMIZED = 3;

        public ApplicationHostService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await HandleActivationAsync();
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates main window during activation.
        /// </summary>
        private async Task HandleActivationAsync()
        {
            const string appName = "POS_ModernUI"; // اسم فريد للتطبيق
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                var current = Process.GetCurrentProcess();
                var other = Process.GetProcessesByName(current.ProcessName)
                                   .FirstOrDefault(p => p.Id != current.Id);

                if (other != null)
                {
                    IntPtr hWnd = other.MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, SW_SHOWMAXIMIZED);   // أو SW_RESTORE
                        SetForegroundWindow(hWnd);
                    }
                }

                App.Current.Shutdown();
                await Task.CompletedTask;
                return;
            }


            if (!Application.Current.Windows.OfType<LoginWindow>().Any())
            {
                _navigationWindow = (
                    _serviceProvider.GetService(typeof(ILoginNavigationWindow)) as ILoginNavigationWindow
                )!;

                _navigationWindow!.ShowWindow();
            }

            await Task.CompletedTask;
        }
    }
}
