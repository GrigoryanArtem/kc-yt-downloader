using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.GUI.Model.Extensions;
using kc_yt_downloader.GUI.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NavigationMVVM.Services;
using NLog.Extensions.Hosting;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace kc_yt_downloader.GUI;

public partial class App : Application
{
    private readonly IHost _host;
    private BrowserExtensionHandler _browserExtensionHandler;

    public App()
    {
        var services = new ServiceCollection();
        var configurator = new DefaultServiceConfigurator();

        var builder = Host.CreateDefaultBuilder();
        _host = builder
            .UseNLog()
            .ConfigureServices(configurator.ConfigureServices)
            .Build();
    }

    public new static App Current => (App)Application.Current;
    public IServiceProvider Services => _host.Services;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var cultureInfo = new CultureInfo("en-us");

        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;

        Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);

        var initialNavigationService = Services.GetRequiredService<NavigationService<UpdateViewModel>>();
        initialNavigationService.Navigate();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        _browserExtensionHandler = Services.GetRequiredService<BrowserExtensionHandler>();
        _browserExtensionHandler.Run();

        mainWindow!.Show();
    }

    protected async override void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }

        await _browserExtensionHandler.Stop();
        base.OnExit(e);
    }

    private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
#if DEBUG && false
        e.Handled = false;
#else
        ShowUnhandledException(e);    
#endif
    }

    private void ShowUnhandledException(DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        var exception = e.Exception;
        var navigation = NavigationCommands.CreateNavigation<string, GlobalErrorViewModel>(s => new(s));

        var messageBuilder = new StringBuilder();

        messageBuilder.AppendLine("UNHANDLED ERROR OCCURRED");
        messageBuilder.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        messageBuilder.AppendLine($"Exception Type: {exception.GetType().FullName}");
        messageBuilder.AppendLine($"Message: {exception.Message}");
        messageBuilder.AppendLine($"Source: {exception.Source}");
        messageBuilder.AppendLine($"Target Site: {exception.TargetSite}");
        messageBuilder.AppendLine($"HResult: {exception.HResult}");        

        foreach(var inner in exception.GetInnerExceptions())
        {
            messageBuilder.AppendLine();
            FormatException(messageBuilder, inner);
        }

        messageBuilder.AppendLine();
        messageBuilder.AppendLine("STACK TRACE:");
        messageBuilder.AppendLine(exception.StackTrace);

        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        messageBuilder.AppendLine();
        messageBuilder.AppendLine($"Application Version: {version}");

        navigation.Navigate(messageBuilder.ToString());
    }

    private void FormatException(StringBuilder builder, Exception exception)
    {
        builder.AppendLine($"---> {exception.GetType().FullName}: {exception.Message}");
        builder.AppendLine($"Source: {exception.Source}");
        builder.AppendLine($"Target Site: {exception.TargetSite}");
        builder.AppendLine($"Stack trace:");
        builder.AppendLine(exception.StackTrace);
    }
}
