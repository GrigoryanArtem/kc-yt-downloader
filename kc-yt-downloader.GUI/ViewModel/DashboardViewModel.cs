using CommunityToolkit.Mvvm.Messaging;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.GUI.Model.Messages;
using kc_yt_downloader.Model;
using NavigationMVVM;
using NavigationMVVM.Services;
using NavigationMVVM.Stores;
using static kc_yt_downloader.GUI.ViewModel.LogViewModel;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class DashboardViewModel : ObservableDisposableObject
    {
        private readonly YtDlp _ytDlp;

        private readonly ParameterNavigationService<CutViewModelParameters, CutViewModel> _cutNavigation;
        private readonly ParameterNavigationService<LogViewModelParameters, LogViewModel> _logNavigation;
        private readonly NavigationService<ObservableDisposableObject> _backNavigation;        

        public DashboardViewModel(NavigationStore store, YtDlp ytDlp)
        {
            _ytDlp = ytDlp;
            _ytDlp.Open();

            UrlAddingViewModel = new(_ytDlp);

            WeakReferenceMessenger.Default.Register<VideosUpdatedMessage>(this, UpdateVideos);
            WeakReferenceMessenger.Default.Register<AddTaskMessage>(this, UpdateTasks);

            WeakReferenceMessenger.Default.Register<DeleteTaskMessage>(this, UpdateTasks);

            _cutNavigation = new ParameterNavigationService<CutViewModelParameters, CutViewModel>(store, cvp => new CutViewModel(cvp));
            _logNavigation = new ParameterNavigationService<LogViewModelParameters, LogViewModel>(store, p => new LogViewModel(p));
            _backNavigation = new NavigationService<ObservableDisposableObject>(store, () => this);            

            UpdateVideos();
            UpdateTasks();
        }

        private IGrouping<DateTime, YTVideoViewModel>[] _videos;
        public IGrouping<DateTime, YTVideoViewModel>[] Videos
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

        private void UpdateTasks(object sender, DeleteTaskMessage message)
        {
            if (message is not null)
                _ytDlp.DeleteTask(message.Task);

            UpdateTasks();
        }

        private void UpdateTasks(object sender, AddTaskMessage message)
        {
            if (message is not null)
                _ytDlp.AddTask(message.Task);

            UpdateTasks();
        }

        private void UpdateTasks()
            => Tasks = _ytDlp.GetCachedTasks()
                .Select(task => new CutTaskViewModel(task, _ytDlp, _cutNavigation, _logNavigation, _backNavigation, _backNavigation))
                .OrderByDescending(video => video.Source.Created)
                .ToArray();

        private void UpdateVideos(object sender, VideosUpdatedMessage message)
            => UpdateVideos();

        private void UpdateVideos()
        {
            Videos = _ytDlp.GetCachedData()
                .OrderByDescending(video => video.ParseDate)
                .Select(video => new YTVideoViewModel(_ytDlp, video, _cutNavigation, _backNavigation, _backNavigation)) 
                .GroupBy(v => new DateTime(year: v.Video.ParseDate.Year, month: v.Video.ParseDate.Month, day: 1))
                .ToArray();
        }
    }
}
