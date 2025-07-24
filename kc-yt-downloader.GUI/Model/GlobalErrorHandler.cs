using kc_yt_downloader.GUI.Model.Extensions;
using kc_yt_downloader.GUI.ViewModel;
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

    public GlobalErrorHandler()    
    {
        Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppUnhandledException;
        TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;
    }

    #region Private methods
    #region Handlers

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        ShowUnhandledException(e.Exception);
        e.Handled = true;
    }

    private void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        ShowUnhandledException(e.Exception);
    }

    private void OnAppUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
#if DEBUG && false
        e.Handled = false;
#else
        ShowUnhandledException(e.ExceptionObject as Exception);
#endif
    }

    #endregion

    private void ShowUnhandledException(Exception? exception)
    {
        var dir = Directory.CreateDirectory(REPORTS_DIRECTORY);
        var reportName = $"report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}";
        var path = Path.Combine(dir.FullName, reportName);


        if (exception is null)
        {
            File.WriteAllText(path, "An unhandled exception occurred, but the exception object is null.");
            return;
        }

        var navigation = NavigationCommands.CreateNavigation<string, GlobalErrorViewModel>(s => new(s));

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

        var report = messageBuilder.ToString();
        File.WriteAllText(path, report);

        NavigationCommands.CloseModal();
        navigation.Navigate(report);
    }

    private void FormatException(StringBuilder builder, Exception exception)
    {
        builder.AppendLine($"---> {exception.GetType().FullName}: {exception.Message}");
        builder.AppendLine($"Source: {exception.Source}");
        builder.AppendLine($"Target Site: {exception.TargetSite}");
        builder.AppendLine($"Stack trace:");
        builder.AppendLine(exception.StackTrace);
    }

    public void Dispose()
    {
        Application.Current.DispatcherUnhandledException -= OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException -= OnAppUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnTaskSchedulerUnobservedTaskException;
    }

    #endregion
}