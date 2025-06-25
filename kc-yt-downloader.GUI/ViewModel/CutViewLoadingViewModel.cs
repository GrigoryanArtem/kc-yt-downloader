using kc_yt_downloader.GUI.Model;

namespace kc_yt_downloader.GUI.ViewModel;

public class CutViewLoadingViewModel : LoadingNextViewViewModel<CutViewModelParameters>
{
    public CutViewLoadingViewModel(Func<Task<CutViewModelParameters>> loadingTask)
    {
        Title = "Loading video metadata...";

        _loadingTask = loadingTask;
        _navigationService = NavigationCommands.CreateNavigation<CutViewModelParameters, CutViewModel>(cvp => new CutViewModel(cvp));
    }
}
