using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavigationMVVM.Services;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class LoadingNextViewViewModel<TParameter> : ObservableObject
{
    protected Func<Task<TParameter>> _loadingTask;
    protected IParameterNavigationService<TParameter> _navigationService;

    [ObservableProperty]
    private bool _isProgress = true;

    public LoadingNextViewViewModel() { }

    public LoadingNextViewViewModel(
        string title,
        Func<Task<TParameter>> loadingTask,
        IParameterNavigationService<TParameter> navigationService) : this()
    {
        _loadingTask = loadingTask;
        _navigationService = navigationService;
        Title = title;
    }

    public string Title { get; protected set; }

    [RelayCommand]
    public async Task Load()
    {
        var parameter = await _loadingTask();

        _navigationService.Navigate(parameter);
    }
}
