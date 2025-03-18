using Wpf.Ui.TaskBar;

namespace SteamToys.Wpfui.Views;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : INavigationWindow
{
    private bool _initialized = false;

    private readonly IThemeService _themeService;

    private readonly ITaskBarService _taskBarService;

    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel, INavigationService navigationService, IPageService pageService, IThemeService themeService, ITaskBarService taskBarService, ICustomSnackbarService snackbarService, IDialogService dialogService)
    {
        ViewModel = viewModel;
        DataContext = this;

        _themeService = themeService;
        _taskBarService = taskBarService;

        InitializeComponent();

        SetPageService(pageService);

        navigationService.SetNavigationControl(RootNavigation);
        snackbarService.SetSnackbarControl(RootSnackbar);
        snackbarService.SetTextBlockControl(RootSnackbarTextBlock);
        dialogService.SetDialogControl(RootDialog);

        Loaded += (_, _) => InvokeSplashScreen();
    }


    private void InvokeSplashScreen()
    {
        if (_initialized)
            return;

        _initialized = true;

        RootMainGrid.Visibility = Visibility.Collapsed;
        RootWelcomeGrid.Visibility = Visibility.Visible;

        _taskBarService.SetState(this, TaskBarProgressState.Indeterminate);

        Task.Run(async () =>
        {
            // Remember to always include Delays and Sleeps in
            // your applications to be able to charge the client for optimizations later.
            await Task.Delay(1000);

            await Dispatcher.InvokeAsync(() =>
            {
                RootWelcomeGrid.Visibility = Visibility.Hidden;
                RootMainGrid.Visibility = Visibility.Visible;

                Navigate(typeof(Pages.Dashboard));

                _taskBarService.SetState(this, TaskBarProgressState.None);
            });

            return true;
        });
    }

    private void RootNavigation_OnNavigated(INavigation sender, RoutedNavigationEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"DEBUG | WPF UI Navigated to: {sender?.Current ?? null}", "Wpf.Ui.Demo");

        // This funky solution allows us to impose a negative
        // margin for Frame only for the Dashboard page, thanks
        // to which the banner will cover the entire page nicely.
        RootFrame.Margin = new Thickness(
            left: 0,
            top:  0,
            right: 0,
            bottom: 0);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Application.Current.Shutdown();
    }

    #region INavigationWindow methods

    public void CloseWindow() => Close();

    public Frame GetFrame() => RootFrame;

    public INavigation GetNavigation() => RootNavigation;

    public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

    public void SetPageService(IPageService pageService) => RootNavigation.PageService = pageService;

    public void ShowWindow() => Show();

    #endregion
}
