using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.Model;
using MaterialDesignThemes.Wpf;
using System.Windows.Media;

namespace kc_yt_downloader.GUI.ViewModel;

public class SimpleStatusViewModel(VideoTaskStatus status) : ObservableObject
{
    public string Status { get; } = status.ToString();

    public Brush StatusColor { get; } = status switch
    {
        VideoTaskStatus.Completed => Brushes.Green,
        VideoTaskStatus.Prepared => Brushes.SandyBrown,
        VideoTaskStatus.Error => Brushes.DarkRed,
        VideoTaskStatus.Cancelled => Brushes.Orange,
        VideoTaskStatus.Unknown => Brushes.Gray,
        VideoTaskStatus.Processing => Brushes.Blue,

        _ => Brushes.Black
    };

    public PackIconKind Icon { get; } = status switch
    {
        VideoTaskStatus.Completed => PackIconKind.CheckCircleOutline,
        VideoTaskStatus.Prepared => PackIconKind.PlayCircleOutline,
        VideoTaskStatus.Error => PackIconKind.AlertCircleOutline,
        VideoTaskStatus.Cancelled => PackIconKind.Cancel,
        VideoTaskStatus.Unknown => PackIconKind.HelpCircleOutline,
        VideoTaskStatus.Processing => PackIconKind.ProgressClock,

        _ => PackIconKind.HelpCircle
    };
}
