using CommunityToolkit.Mvvm.ComponentModel;

namespace kc_yt_downloader.GUI.ViewModel;

public class ErrorInformationViewModel(string details) : ObservableObject
{
    public string Details { get; } = details;
}