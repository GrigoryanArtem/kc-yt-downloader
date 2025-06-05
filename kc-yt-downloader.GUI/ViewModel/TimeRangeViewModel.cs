using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;

namespace kc_yt_downloader.GUI.ViewModel;

public class TimeRangeViewModel : ObservableObject
{
    private static SelectedSettings Settings => YtConfig.Global.SelectedSettings;

    public TimeRangeViewModel(string duration)
    {
        From = "0";
        To = duration;
    }

    public bool FullVideo
    {
        get => Settings.FullVideo;
        set
        {
            SetProperty(Settings.FullVideo, value, nv => Settings.FullVideo = nv);
            OnPropertyChanged(nameof(CanEdit));
        }
    }

    public bool CanEdit => !FullVideo;

    private string _from;
    public string From
    {
        get => _from;
        set => SetProperty(ref _from, value);
    }

    private string _to;
    public string To
    {
        get => _to;
        set => SetProperty(ref _to, value);
    }

    public TimeRange? GetTimeRange()
        => FullVideo ? null : new() { From = From, To = To };
}
