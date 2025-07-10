using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class SettingsViewModel : ObservableValidator
{
    public partial class TaskExpirationTimeViewModel(VideoTaskStatus status, int? expirationTimeDays = null) : ObservableValidator
    {
        [ObservableProperty]
        [Required]
        private int _expirationTimeDays = expirationTimeDays ?? 0;

        [ObservableProperty]
        private bool _infiniteExpirationTime = !expirationTimeDays.HasValue;

        public VideoTaskStatus Status { get; } = status;
    }

    private static readonly HashSet<VideoTaskStatus> EXCLUDED_STATUSES =
    [
        VideoTaskStatus.Processing,
        VideoTaskStatus.Unknown,
        VideoTaskStatus.Cancelled
    ];

    public SettingsViewModel()
    {
        var config = YtConfig.Global;

        BatchSize = config.BatchSize;
        ExpirationTimes = [.. Enum.GetValues<VideoTaskStatus>()
            .Where(s => !EXCLUDED_STATUSES.Contains(s))
            .Select(status => new TaskExpirationTimeViewModel
            (
                status: status,
                expirationTimeDays: config.ExpirationTimes.TryGetValue(status, out var days) ? days : null)
            )];
    }

    [ObservableProperty]
    private int _batchSize;

    public int[] SupportedSize { get; } = [.. Enumerable.Range(2, 4)];    
    public TaskExpirationTimeViewModel[] ExpirationTimes { get; }

    [RelayCommand]
    public void Save()
    {
        try
        {
            var config = YtConfig.Global;
            var services = App.Current.Services;
            var ytDlpProxy = services.GetRequiredService<YtDlpProxy>();

            foreach (var expirationTime in ExpirationTimes)
            {
                if (expirationTime.InfiniteExpirationTime)
                {
                    config.ExpirationTimes.Remove(expirationTime.Status);
                }
                else
                {
                    config.ExpirationTimes[expirationTime.Status] = expirationTime.ExpirationTimeDays;
                }
            }

            config.BatchSize = BatchSize;

            config.Save();
            ytDlpProxy.Sync(YtDlpProxy.SyncType.All);

            GlobalSnackbarMessageQueue.WriteInfo("Settings saved successfully.");            
        }
        catch (Exception ex)
        {
            GlobalSnackbarMessageQueue.WriteError("Error saving settings", ex);
        }
        finally
        {
            NavigationCommands.CloseModal();
        }
    }
}
