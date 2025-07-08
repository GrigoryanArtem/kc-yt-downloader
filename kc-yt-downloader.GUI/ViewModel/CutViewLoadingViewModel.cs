using kc_yt_downloader.GUI.Model;

namespace kc_yt_downloader.GUI.ViewModel;

public class CutViewLoadingViewModel : LoadingNextViewViewModel<CutViewModelParameters>
{
    public CutViewLoadingViewModel(Func<Task<LoadingResult<CutViewModelParameters>>> loadingTask)
    {
        Title = "Loading video metadata...";

        LoadingTask = loadingTask;
        NavigationService = NavigationCommands.CreateNavigation<CutViewModelParameters, CutViewModel>(cvp => new CutViewModel(cvp));
    }
}
