using Serilog;
using SteamToys.Wpfui.Views.Pages;

namespace SteamToys;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IHost host { get; private set; }

    public App()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder();
        BuildSerilogConfig(builder);
        Log.Logger = new LoggerConfiguration()
          .ReadFrom.Configuration(builder.Build())
          .Enrich.FromLogContext()
          .CreateLogger();

        host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(b => { b.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!); })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(context, services);
            })
            .UseSerilog()
            .Build();
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // App Host
        services.AddHostedService<ApplicationHostService>();

        // Page resolver service
        services.AddSingleton<IPageService, PageService>();

        // Theme manipulation
        services.AddSingleton<IThemeService, ThemeService>();

        // TaskBar manipulation
        services.AddSingleton<ITaskBarService, TaskBarService>();

        // Snackbar service
        services.AddSingleton<ICustomSnackbarService, CustomSnackbarService>();

        // Dialog service
        services.AddSingleton<IDialogService, DialogService>();

        // Tray icon
        services.AddSingleton<INotifyIconService, CustomNotifyIconService>();

        // Service containing navigation, same as INavigationWindow... but without window
        services.AddSingleton<INavigationService, NavigationService>();

        // Main window with navigation
        services.AddScoped<INavigationWindow, MainWindow>();
        services.AddScoped<MainWindowViewModel>();

        // Views and ViewModels
        services.AddScoped<Dashboard>();
        services.AddScoped<DashboardViewModel>();

        services.AddScoped<Parameter>();
        services.AddScoped<ParameterViewModel>();

        services.AddScoped<Settings>();
        services.AddScoped<SettingsViewModel>();

        // Business Services
        services.AddSms();
        services.AddTransient<IWorkService, ClientWorkService>();
        services.AddTransient<IMailboxService, MailboxService>();
        services.AddTransient<ISteamClientService, SteamClientService>();
        services.AddScoped<IFileService, FileService>();

       // Configuration
        services.Configure<AppSetting>(context.Configuration);

        // HttpClient
        services.AddHttpClient("sms-activate", c =>
        {
            c.BaseAddress = new Uri($"https://api.sms-activate.org/stubs/");
        });

        services.AddHttpClient("online-sim", c =>
        {
            c.BaseAddress = new Uri("https://onlinesim.io/api/");
        });

        services.AddHttpClient("5-sim", c =>
        {
            c.BaseAddress = new Uri("https://5sim.net/");
        });
    }

    /// <summary>
    /// Occurs when the application is loading.
    /// </summary>
    private async void OnStartup(object sender, StartupEventArgs e) => await host.StartAsync();

    /// <summary>
    /// Occurs when the application is closing.
    /// </summary>
    private async void OnExit(object sender, ExitEventArgs e)
    {
        Log.Error("程序退出了");
        await Log.CloseAndFlushAsync();
        await host.StopAsync();
        host.Dispose();
    }

    /// <summary>
    /// Occurs when an exception is thrown by an application but not handled.
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        Log.Fatal(e.Exception, "未处理异常");
    }

    private void BuildSerilogConfig(IConfigurationBuilder builder)
    {
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.serilog.json", optional: true, reloadOnChange: true);
    }
}
