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
    }
    
    public YtDlpProxy DlpProxy { get; set; }
    

    private CutTaskViewModel[] _tasks;
    public CutTaskViewModel[] Tasks
    {
        get => _tasks;
        set => SetProperty(ref _tasks, value);
    }

    public UrlAddingViewModel UrlAddingViewModel { get; } = new();

    [RelayCommand]
    public void OpenSettings()
    {
        var navigationCommand = NavigationCommands.CreateModalNavigation(new SettingsViewModel());
        navigationCommand.Navigate();
    }
}
