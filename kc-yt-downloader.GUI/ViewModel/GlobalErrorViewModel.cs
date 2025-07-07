using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using System.Windows;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class GlobalErrorViewModel(string details) : ObservableObject
{
    public string Details { get; } = details;

    [RelayCommand]
    public void Close()
    {
        var dashboard = NavigationCommands.CreateNavigation<DashboardViewModel>();
        dashboard.Navigate();
    }

    [RelayCommand]
    public void Copy()
    {
        Clipboard.SetText(Details);
        GlobalSnackbarMessageQueue.WriteInfo("Copied to clipboard");
    }
}
