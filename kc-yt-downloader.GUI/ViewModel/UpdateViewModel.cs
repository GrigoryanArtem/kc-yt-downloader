using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using NavigationMVVM.Services;
using System.Collections.ObjectModel;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class UpdateViewModel : ObservableObject
{
    public class ProgressMessage(string message)
    {
        public string Message { get; } = message ?? string.Empty;
        public DateTime DateTime { get; } = DateTime.Now;
    }

    [ObservableProperty]
    private ObservableCollection<string> _progressMessages = [];

    [RelayCommand]
    public async Task Load()
    {
        var services = App.Current.Services;
        var ytDlp = services.GetRequiredService<YtDlp>();

        var progress = new Progress<string>(msg => ProgressMessages.Insert(0, msg));
        var success = await ytDlp.UpdateYtDlpAsync(YtDlpUpdateChannel.Stable, progress);

        if (!success)
        {
            ProgressMessages.Add("Update failed. Please check the logs for more details.");
            return;
        }

        await Task.Delay((int)TimeSpan.FromSeconds(2).TotalMilliseconds);
        var dashboardNavigation = services.GetRequiredService<NavigationService<DashboardViewModel>>();
        dashboardNavigation.Navigate();
    }
}