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
        private readonly YtDlp _ytDlp = new();

        private readonly ParameterNavigationService<CutViewModelParameters, CutViewModel> _cutNavigation;
        private readonly NavigationService<ObservableDisposableObject> _backNavigation;

        public DashboardViewModel(NavigationStore store)
        {
            _cutNavigation = new ParameterNavigationService<CutViewModelParameters, CutViewModel>(store, cvp => new CutViewModel(cvp));
            _backNavigation = new NavigationService<ObservableDisposableObject>(store, () => this);

            _ytDlp.Open();

            WeakReferenceMessenger.Default.Register<UrlAddedMessage>(this, UpdateVideos);
            UpdateVideos(null, null);
        }

        private YTVideoViewModel[] _videos;
        public YTVideoViewModel[] Videos
        {
            get => _videos;
            set => SetProperty(ref _videos, value);
        }

        public UrlAddingViewModel UrlAddingViewModel { get; } = new();

        private void UpdateVideos(object sender, UrlAddedMessage message)
        {
            Videos = _ytDlp.GetCachedData()
                .Select(info => new YTVideoViewModel(info, _cutNavigation, _backNavigation))
                .ToArray();
        }
    }
}
