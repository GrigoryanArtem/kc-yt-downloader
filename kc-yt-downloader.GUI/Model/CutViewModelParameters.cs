using kc_yt_downloader.Model;
using NavigationMVVM;
using NavigationMVVM.Services;

namespace kc_yt_downloader.GUI.Model
{
    public record CutViewModelParameters
    {
        public CutVideoTask Source { get; set; }
        public VideoInfo VideoInfo { get; init; }
        public NavigationService<ObservableDisposableObject> BackNavigation { get; init; }
        public NavigationService<ObservableDisposableObject> DashboardNavigation { get; init; }
    }
}
