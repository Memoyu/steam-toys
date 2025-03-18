using SteamToys.Controls.Navigation.Common;
using SteamToys.Services.App;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace SteamToys.Services;

public class ApplicationHostService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPageService _pageService;
    private INavigationWindow _navigationWindow;

    public ApplicationHostService(IServiceProvider serviceProvider, IPageService pageService)
    {
        // If you want, you can do something with these services at the beginning of loading the application.
        _serviceProvider = serviceProvider;
        _pageService = pageService;
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
        await Task.CompletedTask;

        if (!Application.Current.Windows.OfType<Container>().Any())
        {
            _navigationWindow = _serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow;
            _navigationWindow!.ShowWindow();

            ControlsServices.Initialize(_serviceProvider);

            _navigationWindow.Navigate(typeof(Dashboard));
        }

        await Task.CompletedTask;
    }
}