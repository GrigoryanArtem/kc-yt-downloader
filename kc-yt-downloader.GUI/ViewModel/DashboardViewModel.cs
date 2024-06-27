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
            WeakReferenceMessenger.Default.Register<AddTaskMessage>(this, UpdateTasks);

            _cutNavigation = new ParameterNavigationService<CutViewModelParameters, CutViewModel>(store, cvp => new CutViewModel(cvp));
            _backNavigation = new NavigationService<ObservableDisposableObject>(store, () => this);            

            UpdateVideos(null, null);
            UpdateTasks(null, null);
        }

        private YTVideoViewModel[] _videos;
        public YTVideoViewModel[] Videos
        {
            get => _videos;
            set => SetProperty(ref _videos, value);
        }

        private CutTaskViewModel[] _tasks;
        public CutTaskViewModel[] Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        public UrlAddingViewModel UrlAddingViewModel { get; }

        private void UpdateTasks(object sender, AddTaskMessage message)
        {
            if (message is not null)
            {
                _ytDlp.AddTask(message.Task);
            }

            Tasks = _ytDlp.GetCachedTasks()
                .Select(task => new CutTaskViewModel(task, _ytDlp, _cutNavigation, _backNavigation, _backNavigation))
                .OrderByDescending(video => video.Source.Created)
                .ToArray();
        }

        private void UpdateVideos(object sender, UrlAddedMessage message)
        {
            Videos = _ytDlp.GetCachedData()
                .OrderByDescending(video => video.ParseDate)
                .Select(video => new YTVideoViewModel(video, _cutNavigation, _backNavigation, _backNavigation))                
                .ToArray();
        }
    }
}
