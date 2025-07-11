using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

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

    [ObservableProperty]
    private int _batchSize;
    [ObservableProperty]
    private string _ytDlpVersion;
    [ObservableProperty]
    private string _ffmpegVersion;
    [ObservableProperty]
    private string _appVersion;

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

        
        Task.Run(LoadVersions);
        
    }

    public async Task LoadVersions()
    {
        var services = App.Current.Services;

        var ytDlp = services.GetRequiredService<YtDlp>();
        var ffmpeg = services.GetRequiredService<FFmpeg>();

        YtDlpVersion = await ytDlp.YtDlpVersion(CancellationToken.None);
        FfmpegVersion = await ffmpeg.FFmpegVersion(CancellationToken.None);
        AppVersion = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString()?? String.Empty;
    }


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
