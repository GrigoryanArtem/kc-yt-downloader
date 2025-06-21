using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.GUI.ViewModel;
using kc_yt_downloader.Model;
using System.Collections.ObjectModel;

namespace kc_yt_downloader.GUI.Model;

public partial class YtDlpProxy : ObservableObject
{
    public class VideoGroupViewModel(DateTime date) : ObservableObject
    {
        public DateTime Date { get; } = date;
        public ObservableCollection<YTVideoViewModel> Items { get; } = [];
    }

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

    #endregion

    public YtDlpProxy(YtDlp ytDlp)
    {
        _ytDlp = ytDlp;
        Sync(SyncType.All);
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
                SyncGroupItems(existing.Items, [.. group], (a, b) => a.Video.Id == b.Video.Id);
            }
        }
    }

    private void SyncGroupItems<T>(
        ObservableCollection<T> collection,
        T[] updated,
        Func<T, T, bool> identitySelector)
    {
        var toAdd = updated
            .Where(item => !collection.Any(existing => identitySelector(existing, item)))
            .ToArray();

        var toRemove = collection
            .Where(existing => !updated.Any(newItem => identitySelector(existing, newItem)))
            .ToArray();

        Array.ForEach(toRemove, item => collection.Remove(item));
        Array.ForEach(toAdd, item => collection.Insert(0, item));      
        
        for (int i = 0; i < updated.Length; i++)
        {
            var desiredItem = updated[i];
            var currentIndex = collection.IndexOf(desiredItem);

            if (currentIndex != i && currentIndex >= 0)
                collection.Move(currentIndex, i);
        }
    }

    #endregion
}
