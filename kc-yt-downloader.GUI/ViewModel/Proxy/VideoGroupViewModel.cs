using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace kc_yt_downloader.GUI.ViewModel.Proxy;

public class VideoGroupViewModel(DateTime date) : ObservableObject
{
    public DateTime Date { get; } = date;
    public ObservableCollection<YTVideoViewModel> Items { get; } = [];
}
