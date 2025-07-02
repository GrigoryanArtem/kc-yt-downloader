using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.GUI.Model.Extensions;
using kc_yt_downloader.GUI.ViewModel;
using kc_yt_downloader.GUI.ViewModel.Proxy;
using kc_yt_downloader.Model;
using System.Collections.ObjectModel;

namespace kc_yt_downloader.GUI.Model;

public partial class YtDlpProxy : ObservableObject
{
    [Flags]
    public enum SyncType
    {
        Videos = 1,
        Tasks = 2,

        All = Videos | Tasks
    }

    #region Members

    private readonly YtDlp _ytDlp;

    #endregion

    #region Properties

    [ObservableProperty]
    private ObservableCollection<VideoGroupViewModel> _videos = [];

    [ObservableProperty]
    private ObservableCollection<TaskGroupViewModel> _tasks = [];

    #endregion

    public YtDlpProxy(YtDlp ytDlp)
    {
        _ytDlp = ytDlp;
        Sync(SyncType.All);
    }

    public void AddTasks(params CutVideoTask[] tasks)
    {
        if (tasks is null)
            return;

        try
        {
            foreach (var task in tasks.Where(t => t is not null))                            
                _ytDlp.AddTask(task);            

            Sync(SyncType.Tasks);
            GlobalSnackbarMessageQueue.WriteInfo($"Added {tasks.Length} tasks.");
        }
        catch (Exception ex)
        {
            GlobalSnackbarMessageQueue.WriteError("Failed to add tasks.", ex);
            return;
        }
    }

    public void DeleteTask(CutVideoTask? task)
    {
        if (task is null)
            return;
        try
        {
            _ytDlp.DeleteTask(task);
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
            _ytDlp.DeleteVideo(url);
            Sync(SyncType.Videos);

            GlobalSnackbarMessageQueue.WriteInfo($"Deleted video: {url.Info.OriginalUrl}");
        }
        catch (Exception ex)
        {
            GlobalSnackbarMessageQueue.WriteError($"Failed to delete video: {url.Info.OriginalUrl}", ex);
            return;
        }
    }

    public Video? GetVideo(string url)
    {
        try
        {
            var video = _ytDlp.GetVideoByUrl(url);
            Sync(SyncType.Videos);

            GlobalSnackbarMessageQueue.WriteInfo($"Added video from URL: {url}");

            return video;
        }
        catch (Exception ex)
        {
            GlobalSnackbarMessageQueue.WriteError($"Failed to add video from URL: {url}", ex);
            return null;
        }
    }

    public void Sync(SyncType type) => App.Current.Dispatcher.Invoke(() =>
    {
        if (type.HasFlag(SyncType.Videos))
            UpdateVideos();

        if (type.HasFlag(SyncType.Tasks))
            UpdateTasks();
    });

    #region Private methods

    private void UpdateVideos()
    {
        var newVideos = _ytDlp.GetCachedData()
            .OrderByDescending(v => v.ParseDate)
            .Select(v => new YTVideoViewModel(_ytDlp, v))
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
        var newTasks = _ytDlp.GetCachedTasks()
            .Where(t => t.Status != VideoTaskStatus.Completed || t.Created > DateTime.Now.AddDays(-3))
            .Select(t => new CutTaskViewModel(t, _ytDlp))
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

                int insertAt = Tasks.TakeWhile(g => g.Status > group.Key).Count();
                Tasks.Insert(insertAt, newGroup);
            }
            else
            {
                existing.Items.SyncItems(group, (a, b) => a.Source.Id == b.Source.Id);
            }
        }
    }

    #endregion
}
