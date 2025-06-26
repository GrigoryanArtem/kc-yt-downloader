using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;
using System.Collections.ObjectModel;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class TimeRangeViewModel(TimeRange[]? segments, string durationString) : ObservableObject
{
    private static SelectedSettings Settings => YtConfig.Global.SelectedSettings;

    public bool FullVideo
    {
        get => Settings.FullVideo;
        set
        {
            SetProperty(Settings.FullVideo, value, nv => Settings.FullVideo = nv);
            OnPropertyChanged(nameof(CanEdit));
        }
    }

    public ObservableCollection<SegmentViewModel> Segments { get; } 
        = segments is not null && segments.Length > 0
            ? [..segments.Select(s => new SegmentViewModel(s))]
            : [new(durationString)];

    public bool CanEdit => !FullVideo;
    public bool MultipleSegments => Segments.Count > 1;

    public TimeRange?[] GetTimeRanges()
        => FullVideo ? [ null! ] : Segments.Select(s => new TimeRange() { From = s.From, To = s.To }).ToArray();

    [RelayCommand]
    public void AddSegment()
    {
        Segments.Add(new(durationString));
        OnPropertyChanged(nameof(MultipleSegments));
    }

    [RelayCommand]
    public void DeleteSegment(SegmentViewModel? segment)
    {
        if (segment is null)
            return;

        Segments.Remove(segment);    
        OnPropertyChanged(nameof(MultipleSegments));
    }
}
