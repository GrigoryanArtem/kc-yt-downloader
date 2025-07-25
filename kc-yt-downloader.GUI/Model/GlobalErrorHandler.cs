using kc_yt_downloader.GUI.Model.Extensions;
using kc_yt_downloader.GUI.ViewModel;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace kc_yt_downloader.GUI.Model;

public class GlobalErrorHandler : IDisposable
{
    #region Constants

    private const string REPORTS_DIRECTORY = "reports";

    #endregion

    private readonly ILogger<GlobalErrorHandler> _logger;

    public GlobalErrorHandler(ILogger<GlobalErrorHandler> logger)
    {
        _logger = logger;

#if ENABLE_GLOBAL_EXCEPTION_LOGGING
        _logger.LogInformation("Global exception logging is enabled. Subscribing to unhandled exception events.");

        Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppUnhandledException;
        TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;
#endif
    }

    #region Private methods
    #region Handlers

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "Unhandled exception in the dispatcher.");

        ShowUnhandledException(e.Exception);
        e.Handled = true;
    }

    private void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "Unobserved task exception occurred.");

        ShowUnhandledException(e.Exception);
    }

    private void OnAppUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _logger.LogError(e.ExceptionObject as Exception, "Unhandled exception in the application domain.");

        ShowUnhandledException(e.ExceptionObject as Exception);
    }

    #endregion

    private void ShowUnhandledException(Exception? exception)
    {
        _logger.LogInformation("Showing unhandled exception dialog.");

        var dir = Directory.CreateDirectory(REPORTS_DIRECTORY);
        var reportName = $"report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}";
        var path = Path.Combine(dir.FullName, reportName);

        if (exception is null)
        {
            File.WriteAllText(path, "An unhandled exception occurred, but the exception object is null.");
            _logger.LogInformation("No exception object provided, wrote an empty report: {path}.", path);
            return;
        }

        var report = CreateReport(exception);
        File.WriteAllText(path, report);
        _logger.LogInformation("Error report created at: {path}.", path);

        var navigation = NavigationCommands.CreateNavigation<string, GlobalErrorViewModel>(s => new(s));
        NavigationCommands.CloseModal();
        navigation.Navigate(report);
    }

    public void Dispose()
    {
#if ENABLE_GLOBAL_EXCEPTION_LOGGING
        Application.Current.DispatcherUnhandledException -= OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException -= OnAppUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnTaskSchedulerUnobservedTaskException;
#endif
    }

    private string CreateReport(Exception exception)
    {
        _logger.LogInformation("Creating error report.");

        var messageBuilder = new StringBuilder();
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;

        messageBuilder.AppendLine("UNHANDLED ERROR OCCURRED");
        messageBuilder.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        messageBuilder.AppendLine($"Application Version: {version}");
        messageBuilder.AppendLine($"Exception Type: {exception.GetType().FullName}");
        messageBuilder.AppendLine($"----------------------");
        messageBuilder.AppendLine($"Message: {exception.Message}");
        messageBuilder.AppendLine($"Source:  {exception.Source}");
        messageBuilder.AppendLine($"Target:  {exception.TargetSite}");
        messageBuilder.AppendLine($"HResult: {exception.HResult}");

        foreach (var inner in exception.GetInnerExceptions())
        {
            messageBuilder.AppendLine();
            FormatException(messageBuilder, inner);
        }

        messageBuilder.AppendLine();
        messageBuilder.AppendLine("STACK TRACE:");
        messageBuilder.AppendLine(exception.StackTrace);

        messageBuilder.AppendLine();

        _logger.LogInformation("Error report created successfully.");

        return messageBuilder.ToString();
    }

    private static void FormatException(StringBuilder builder, Exception exception)
    {
        builder.AppendLine($"---> {exception.GetType().FullName}: {exception.Message}");
        builder.AppendLine($"Source: {exception.Source}");
        builder.AppendLine($"Target Site: {exception.TargetSite}");
        builder.AppendLine($"Stack trace:");
        builder.AppendLine(exception.StackTrace);
    }

    #endregion
}