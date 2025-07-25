using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.Model;
using System.Collections.ObjectModel;

namespace kc_yt_downloader.GUI.ViewModel.Proxy;

public class TaskGroupViewModel(VideoTaskStatus status) : ObservableObject
{
    public VideoTaskStatus Status { get; } = status;
    public ObservableCollection<CutTaskViewModel> Items { get; } = [];
}
