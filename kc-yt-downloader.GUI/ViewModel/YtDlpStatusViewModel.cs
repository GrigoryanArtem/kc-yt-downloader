using NavigationMVVM;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class YtDlpStatusViewModel : ObservableDisposableObject
    {
        

        private string _frame;
        public string Frame 
        {
            get => _frame;
            set => SetProperty(ref _frame, value);
        }

        private string _time;
        public string Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        private string _fps;
        public string FPS
        {
            get => _fps;
            set => SetProperty(ref _fps, value);
        }

        private string _size;
        public string Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        private string _speed;
        public string Speed
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }

        private string _bitRate;
        public string BitRate
        {
            get => _bitRate;
            set => SetProperty(ref _bitRate, value);
        }
    }
}
