using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.GUI.Model.Extensions;
using kc_yt_downloader.GUI.ViewModel;
using kc_yt_downloader.GUI.ViewModel.Proxy;
using kc_yt_downloader.Model;
using kc_yt_downloader.Model.Tasks;
using System.Collections.ObjectModel;

namespace kc_yt_downloader.GUI.Model;

public partial class YtDlpProxy(YtDlp ytDlp) : ObservableObject
{
    [Flags]
    public enum SyncType
    {
        Videos = 1,
        Tasks = 2,

        All = Videos | Tasks
    }

    #region Members

    private readonly Dictionary<long, CutTaskViewModel> _taskCache = [];

    #endregion

    #region Properties

    [ObservableProperty]
    private bool _isAutoProcessingPossible;

    [ObservableProperty]
    private ObservableCollection<VideoGroupViewModel> _videos = [];

    [ObservableProperty]
    private ObservableCollection<TaskGroupViewModel> _tasks = [];

    public Dictionary<string, float> _avgFormatSpeed = [];

    #endregion

    public bool TryGetAvgFormatSpeed(string formatString, out float avgSpeed)
    {
        var hasValue = _avgFormatSpeed.TryGetValue(formatString, out var speed);
        avgSpeed = hasValue ? speed : 0f;

        return hasValue;
    }

    public void AddTasks(params DownloadVideoTask[] tasks)
    {
        if (tasks is null)
            return;

        try
        {
            foreach (var task in tasks.Where(t => t is not null))
                ytDlp.AddTask(task);

            Sync(SyncType.Tasks);
            GlobalSnackbarMessageQueue.WriteInfo($"Added {tasks.Length} tasks.");
        }
        catch (Exception ex)
        {
            GlobalSnackbarMessageQueue.WriteError("Failed to add tasks.", ex);
            return;
        }
    }

    public void DeleteTask(DownloadVideoTask? task)
    {
        if (task is null)
            return;
        try
        {
            ytDlp.DeleteTask(task);
            Sync(SyncType.Tasks);
            GlobalSnackbarMessageQueue.WriteInfo($"Deleted task: {task.Name}");
        }
        catch (Exception ex)
        {
            GlobalSnackbarMessageQueue.WriteError($"Failed to delete task: {task.Name}", ex);
            return;
        }
    }

    public void DeleteVideo(VideoPreview? url)
    {
        if (url is null)
            return;

        try
        {
            ytDlp.DeleteVideo(url);
            Sync(SyncType.Videos);

            GlobalSnackbarMessageQueue.WriteInfo($"Deleted video: {url.Info.OriginalUrl}");
        }
        catch (Exception ex)
        {
            GlobalSnackbarMessageQueue.WriteError($"Failed to delete video: {url.Info.OriginalUrl}", ex);
            return;
        }
    }

    public async Task<Video?> GetVideo(string url, CancellationToken cancellationToken)
    {
        try
        {
            var video = await ytDlp.GetVideoByUrl(url, cancellationToken);
            Sync(SyncType.Videos);

            GlobalSnackbarMessageQueue.WriteInfo($"Added video from URL: {url}");

            return video;
        }
        catch (Exception ex)
        {
            GlobalSnackbarMessageQueue.WriteError($"Failed to add video from URL: {url}", ex);
            return null!;
        }
    }

    public void Sync(SyncType type) => App.Current.Dispatcher.Invoke(() =>
    {
        if (type.HasFlag(SyncType.Videos))
            UpdateVideos();

        if (type.HasFlag(SyncType.Tasks))
            UpdateTasks();
    });

    public CutTaskViewModel[] GetCachedTasks()
        => [.. ytDlp.GetCachedTasks()
            .Select(GetViewModel)
            .OrderByDescending(t => t.Source.Created)];

    #region Private methods

    private void UpdateVideos()
    {
        var newVideos = ytDlp.GetCachedData()
            .OrderByDescending(v => v.ParseDate)
            .Select(v => new YTVideoViewModel(ytDlp, v))
            .GroupBy(vm => new DateTime
            (
                year: vm.Video.ParseDate.Year,
                month: vm.Video.ParseDate.Month,
                day: 1))
            .ToArray();

        var toRemove = Videos
            .Where(group => !newVideos.Any(ng => ng.Key == group.Date))
            .ToArray();

        foreach (var group in toRemove)
            Videos.Remove(group);

        foreach (var group in newVideos)
        {
            var existing = Videos.FirstOrDefault(g => g.Date == group.Key);
            if (existing == null)
            {
                var newGroup = new VideoGroupViewModel(group.Key);

                foreach (var video in group)
                    newGroup.Items.Add(video);

                int insertAt = Videos.TakeWhile(g => g.Date > group.Key).Count();
                Videos.Insert(insertAt, newGroup);
            }
            else
            {
                existing.Items.SyncItems(group, (a, b) => a.Video.Id == b.Video.Id);
            }
        }
    }

    private void UpdateTasks()
    {
        var config = YtConfig.Global;
        var expirationTimes = config.ExpirationTimes;
        var cachedTasks = ytDlp.GetCachedTasks();

        _avgFormatSpeed = cachedTasks
            .Where(t => t.SpeedMedian.HasValue && !String.IsNullOrEmpty(t.VideoFormatId) && !String.IsNullOrEmpty(t.AudioFormatId))
            .GroupBy(t => t.FormatString)
            .ToDictionary(g => g.Key, g => g.Average(t => t.SpeedMedian!.Value));

        var newTasks = cachedTasks
            .Where(t => !expirationTimes.TryGetValue(t.Status, out var time) || t.Created > DateTime.Now.AddDays(-time))
            .Select(GetViewModel)
            .OrderByDescending(t => t.Source.Completed ?? t.Source.Created)
            .GroupBy(vm => vm.Source.Status)
            .ToArray();

        var toRemove = Tasks
            .Where(group => !newTasks.Any(ng => ng.Key == group.Status))
            .ToArray();

        foreach (var group in toRemove)
            Tasks.Remove(group);

        foreach (var group in newTasks)
        {
            var existing = Tasks.FirstOrDefault(g => g.Status == group.Key);
            if (existing == null)
            {
                var newGroup = new TaskGroupViewModel(group.Key);
                foreach (var task in group)
                    newGroup.Items.Add(task);

                int insertAt = Tasks.TakeWhile(g => GetOrder(g.Status) < GetOrder(group.Key)).Count();
                Tasks.Insert(insertAt, newGroup);
            }
            else
            {
                existing.Items.SyncItems(group, (a, b) => a.Source.Id == b.Source.Id);
            }
        }

        IsAutoProcessingPossible = Tasks.Any(g => g.Status == VideoTaskStatus.Prepared);
    }

    public CutTaskViewModel GetViewModel(DownloadVideoTask task)
        => _taskCache.GetOrAdd(task.Id, () => new CutTaskViewModel(task, this));

    private static int GetOrder(VideoTaskStatus status) => status switch
    {
        VideoTaskStatus.Processing => 0,
        VideoTaskStatus.Prepared => 10,
        VideoTaskStatus.Error => 100,
        VideoTaskStatus.Completed => 10000,
        _ => 1000
    };

    #endregion
}
