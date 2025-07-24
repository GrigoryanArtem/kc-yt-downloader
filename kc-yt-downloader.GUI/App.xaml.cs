using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.GUI.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NavigationMVVM.Services;
using NLog;
using NLog.Extensions.Logging;
using System.Globalization;
using System.Windows;

namespace kc_yt_downloader.GUI;

public partial class App : Application
{
    private readonly IHost _host;
    private readonly BrowserExtensionHandler _browserExtensionHandler;
    private readonly GlobalErrorHandler _globalErrorHandler;
    private readonly ILogger<App> _logger;

    public App()
    {
        var services = new ServiceCollection();
        var configurator = new DefaultServiceConfigurator();
        var builder = Host.CreateDefaultBuilder();

        _host = builder
             .ConfigureLogging(logging =>
             {
                 logging.ClearProviders();
                 logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                 logging.AddNLog(new NLogProviderOptions
                 {
                     CaptureMessageTemplates = true,
                     CaptureMessageProperties = true
                 });
             })
            .ConfigureServices(configurator.ConfigureServices)
            .Build();

        _globalErrorHandler = _host.Services.GetRequiredService<GlobalErrorHandler>();
        _browserExtensionHandler = Services.GetRequiredService<BrowserExtensionHandler>();
        _logger = Services.GetRequiredService<ILogger<App>>();

        SetupCulture("en-us");
        _logger.LogInformation("Host created...");
    }

    public new static App Current => (App)Application.Current;
    public IServiceProvider Services => _host.Services;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        _logger.LogInformation("Application starting...");

        var mainWindow = Services.GetRequiredService<MainWindow>();
        var initialNavigationService = Services.GetRequiredService<NavigationService<UpdateViewModel>>();
        initialNavigationService.Navigate();

        _browserExtensionHandler.Run();
        mainWindow!.Show();

        _logger.LogInformation("Application started successfully.");
    }

    protected async override void OnExit(ExitEventArgs e)
    {
        _logger.LogInformation("Application exiting...");

        using (_host)
        {
            await _host.StopAsync();
        }

        await _browserExtensionHandler.Stop();

        LogManager.Shutdown();
        _globalErrorHandler.Dispose();

        base.OnExit(e);
    }

    private void SetupCulture(string name)
    {
        var cultureInfo = new CultureInfo(name);

        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;

        _logger.LogInformation("Culture set to {cultureName} ({cultureEnglishName})", cultureInfo.Name, cultureInfo.EnglishName);
    }
}
