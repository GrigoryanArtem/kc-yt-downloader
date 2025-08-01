using CommunityToolkit.Mvvm.ComponentModel;

namespace kc_yt_downloader.GUI.Model;

public partial class Selectable<T> : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;
    public required T Item { get; init; }

    public static Selectable<T> Convert(T item)
        => new() { Item = item };
}
