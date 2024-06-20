using kc_yt_downloader.Model;
using NavigationMVVM;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class TimeRangeViewModel : ObservableDisposableObject
    {
        public TimeRangeViewModel(string duration)
        {
            From = "0";
            To = duration;
        }

        private bool _fullVideo;
        public bool FullVideo
        {
            get => _fullVideo;
            set
            {
                SetProperty(ref _fullVideo, value);
                OnPropertyChanged(nameof(CanEdit));
            }
        }

        public bool CanEdit => !FullVideo;

        private string _from;
        public string From
        {
            get => _from; 
            set => SetProperty(ref _from, value);
        }

        private string _to;
        public string To
        {
            get => _to;
            set => SetProperty(ref _to, value);
        }

        public TimeRange? GetTimeRange()
            => FullVideo ? null : new() { From = From, To = To };
    }
}
