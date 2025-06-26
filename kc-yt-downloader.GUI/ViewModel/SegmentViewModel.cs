using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.Model;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class SegmentViewModel : ObservableObject
{
    #region Observable properties

    [ObservableProperty]
    private string _from;

    [ObservableProperty]
    private string _to;

    [ObservableProperty]
    private string _suffix;

    #endregion

    public SegmentViewModel(TimeRange range)
    {
        From = range.From;
        To = range.To;
    }

    public SegmentViewModel(string from, string to)
    {
        From = from;
        To = to;
    }

    public SegmentViewModel(string duration)
    {
        From = "0";
        To = duration;
    }

}
