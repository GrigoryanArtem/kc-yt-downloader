using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using NavigationMVVM.Stores;

namespace kc_yt_downloader.GUI.ViewModel;

public class DashboardViewModel : ObservableObject
{
    private readonly YtDlp _ytDlp;

    public DashboardViewModel(NavigationStore store, YtDlp ytDlp)
    {
        var services = App.Current.Services;

        _ytDlp = ytDlp;
        DlpProxy = services.GetRequiredService<YtDlpProxy>();
    }
    
    public YtDlpProxy DlpProxy { get; set; }
    

    private CutTaskViewModel[] _tasks;
    public CutTaskViewModel[] Tasks
    {
        get => _tasks;
        set => SetProperty(ref _tasks, value);
    }

    public UrlAddingViewModel UrlAddingViewModel { get; } = new();
}
