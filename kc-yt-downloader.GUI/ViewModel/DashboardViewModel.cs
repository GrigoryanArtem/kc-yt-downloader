using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using Microsoft.Extensions.DependencyInjection;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class DashboardViewModel : ObservableObject
{
    public DashboardViewModel()
    {
        var services = App.Current.Services;

        DlpProxy = services.GetRequiredService<YtDlpProxy>();
        DlpProxy.Sync(YtDlpProxy.SyncType.All);

        TaskRunner = new(DlpProxy);
    }

    public YtDlpProxy DlpProxy { get; set; }
    public AutoTaskRunner TaskRunner { get; }
    public UrlAddingViewModel UrlAddingViewModel { get; } = new();

    [RelayCommand]
    public void OpenDrafts()
    {
        var navigation = NavigationCommands.CreateModalNavigation<DraftsListViewModel>();
        navigation.Navigate();
    }

    [RelayCommand]
    public void OpenSettings()
    {
        var navigationCommand = NavigationCommands.CreateModalNavigation(new SettingsViewModel());
        navigationCommand.Navigate();
    }

    [RelayCommand]
    public void RunSingle()
    {
        TaskRunner.Activate(AutoTaskRunner.RunType.Single);

    }

    [RelayCommand]
    public void RunBatch()
    {
        TaskRunner.Activate(AutoTaskRunner.RunType.Batch);
    }

    [RelayCommand]
    public void Stop()
    {
        TaskRunner.Stop();
    }
}
