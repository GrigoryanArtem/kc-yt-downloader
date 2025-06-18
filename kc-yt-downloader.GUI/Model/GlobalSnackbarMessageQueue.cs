using kc_yt_downloader.GUI.ViewModel;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using NavigationMVVM.Services;
using System.Text;

namespace kc_yt_downloader.GUI.Model;

public static class GlobalSnackbarMessageQueue
{
    public static SnackbarMessageQueue Queue { get; }
        = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));

    public static void WriteInfo(string message)
        => Queue.Enqueue(message);

    public static void WriteError(string message, Exception exception)
    {
        var navigation = App.Current.Services.GetRequiredService<ParameterNavigationService<string, ErrorInformationViewModel>>();

        var messageBuilder = new StringBuilder();

        messageBuilder.AppendLine($"Type: {exception.GetType()}");
        messageBuilder.AppendLine();
        messageBuilder.AppendLine("Error information:");
        messageBuilder.AppendLine(exception.InnerException?.Message ?? exception.Message);
        messageBuilder.AppendLine();
        messageBuilder.AppendLine("Stack Trace:");
        messageBuilder.AppendLine(exception.StackTrace);

        Queue.Enqueue(
            content: message,
            actionContent: "Details",
            actionHandler: _ => navigation.Navigate(messageBuilder.ToString()),
            actionArgument: default,
            promote: true,
            neverConsiderToBeDuplicate: false,
            durationOverride: TimeSpan.FromSeconds(6)
        );
    }
}
