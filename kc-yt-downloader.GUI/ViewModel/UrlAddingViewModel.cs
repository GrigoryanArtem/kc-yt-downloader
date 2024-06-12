using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using kc_yt_downloader.GUI.Model.Messages;
using kc_yt_downloader.Model;
using NavigationMVVM;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class UrlAddingViewModel : ObservableDisposableObject
    {
        public UrlAddingViewModel()
        {
            AddUrlCommand = new RelayCommand(async () => await OnAddUrlAsync());
        }

        private string? _url;
        public string? Url
        {
            get => _url;
            set
            {
                SetProperty(ref _url, value);
                OnPropertyChanged(nameof(IsAddButtonEnable));
            }
        }
                
        public bool IsAddButtonEnable => !IsProgress && !String.IsNullOrEmpty(Url);

        private bool _isProgress;
        public bool IsProgress 
        {
            get => _isProgress;
            set
            {
                SetProperty(ref _isProgress, value);
                OnPropertyChanged(nameof(IsAddButtonEnable));
            }
        }
        public RelayCommand AddUrlCommand { get; }

        public Task OnAddUrlAsync()
            => Task.Run(OnAddUrl);

        public void OnAddUrl()
        {
            IsProgress = true;

            var url = Url;
            Url = String.Empty;

            Task.Delay(3000).Wait();

            var ytdlp = new YtDlp();
            ytdlp.Open();
            ytdlp.GetInfo(url);

            WeakReferenceMessenger.Default.Send(new UrlAddedMessage());

            IsProgress = false;
        }
    }
}
