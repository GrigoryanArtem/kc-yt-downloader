using CommunityToolkit.Mvvm.Messaging;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.GUI.Model.Messages;
using kc_yt_downloader.Model;
using NavigationMVVM;
using NavigationMVVM.Services;
using NavigationMVVM.Stores;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class DashboardViewModel : ObservableDisposableObject
    {
        private readonly YtDlp _ytDlp;

        private readonly ParameterNavigationService<CutViewModelParameters, CutViewModel> _cutNavigation;
        private readonly NavigationService<ObservableDisposableObject> _backNavigation;

        public DashboardViewModel(NavigationStore store, YtDlp ytDlp)
        {
            _ytDlp = ytDlp;
            _ytDlp.Open();

            UrlAddingViewModel = new(_ytDlp);
            WeakReferenceMessenger.Default.Register<UrlAddedMessage>(this, UpdateVideos);

            _cutNavigation = new ParameterNavigationService<CutViewModelParameters, CutViewModel>(store, cvp => new CutViewModel(cvp));
            _backNavigation = new NavigationService<ObservableDisposableObject>(store, () => this);

            UpdateVideos(null, null);
        }

        private YTVideoViewModel[] _videos;
        public YTVideoViewModel[] Videos
        {
            get => _videos;
            set => SetProperty(ref _videos, value);
        }

        public UrlAddingViewModel UrlAddingViewModel { get; }

        private void UpdateVideos(object sender, UrlAddedMessage message)
        {
            Videos = _ytDlp.GetCachedData()
                .Select(video => new YTVideoViewModel(video, _cutNavigation, _backNavigation))
                .OrderByDescending(video => video.UploadDate)
                .ToArray();
        }
    }
}
