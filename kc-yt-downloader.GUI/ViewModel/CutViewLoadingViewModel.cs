using kc_yt_downloader.GUI.Model;
using Microsoft.Extensions.DependencyInjection;
using NavigationMVVM.Services;
using NavigationMVVM.Stores;

namespace kc_yt_downloader.GUI.ViewModel;
public class CutViewLoadingViewModel : LoadingNextViewViewModel<CutViewModelParameters>
{
    public CutViewLoadingViewModel(Func<Task<CutViewModelParameters>> loadingTask)
    {
        Title = "Loading video metadata...";

        _loadingTask = loadingTask;

        var services = App.Current.Services;
        var store = services.GetService<NavigationStore>();

        _navigationService = new ParameterNavigationService<CutViewModelParameters, CutViewModel>(store, cvp => new CutViewModel(cvp));
    }
}
