using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using NavigationMVVM.Services;

namespace kc_yt_downloader.GUI.ViewModel;

public abstract partial class LoadingNextViewViewModel<TParameter> : ObservableObject
{    
    [ObservableProperty]
    private bool _isProgress = true;

    public LoadingNextViewViewModel() { }
   
    public LoadingNextViewViewModel(
        string title,
        Func<Task<LoadingResult<TParameter>>> loadingTask,
        IParameterNavigationService<TParameter> navigationService) : this()
    {
        LoadingTask = loadingTask;
        NavigationService = navigationService;
        Title = title;
    }

    protected Func<Task<LoadingResult<TParameter>>> LoadingTask { get; set; }
    protected IParameterNavigationService<TParameter> NavigationService { get; set; }

    public string Title { get; protected set; }

    [RelayCommand]
    public async Task Load()
    {
        var parameter = await LoadingTask();

        if (parameter.Success)
        {
            NavigationService.Navigate(parameter.Result);
        }
        else
        {
            NavigationCommands.NavigateBack();
        }
    }
}
