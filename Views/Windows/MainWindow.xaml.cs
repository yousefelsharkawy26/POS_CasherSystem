using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Abstractions;
using POS_ModernUI.ViewModels.Windows;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.Services.ImageServices.Interface;

namespace POS_ModernUI.Views.Windows;
public partial class MainWindow : IMainNavigationWindow
{
    #region Fields
    private IImageRemoverService _imageRemover;
    #endregion

    #region Props
    public MainWindowViewModel ViewModel { get; }
    #endregion

    #region Constructors
    public MainWindow(
        MainWindowViewModel viewModel,
        INavigationViewPageProvider navigationViewPageProvider,
        INavigationService navigationService,
        IImageRemoverService imageRemover,
        IDialogService dialogService)
    {
        ViewModel = viewModel;
        DataContext = this;
        _imageRemover = imageRemover;

        SystemThemeWatcher.Watch(this);
        InitializeComponent();
        SetPageService(navigationViewPageProvider);

        navigationService.SetNavigationControl(RootNavigation);

        dialogService.SetContentPresenter(ContentPresenter);
    }
    #endregion

    #region INavigationWindow methods

    public INavigationView GetNavigation() => RootNavigation;

    public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

    public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) 
        => RootNavigation.SetPageProviderService(navigationViewPageProvider);

    public void ShowWindow() => Show();

    public void CloseWindow() => Close();

    INavigationView INavigationWindow.GetNavigation()
    {
        throw new NotImplementedException();
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }

    #endregion INavigationWindow methods
    
    #region ApplicationClose
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        GC.Collect();
        GC.WaitForPendingFinalizers();

        _imageRemover.LazyRemoveImagesFromPaths();
        // Make sure that closing this window will begin the process of closing the application.
        Application.Current.Shutdown();
    }
    #endregion
}
