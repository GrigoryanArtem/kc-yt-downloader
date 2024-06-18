using NavigationMVVM;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class RecodeViewModel : ObservableDisposableObject
    {
        private readonly static string[] FORMATS = 
        [
            "avi", "flv", "gif", "mkv", "mov", 
            "mp4", "webm", "aac", "aiff", "alac", 
            "flac", "m4a", "mka", "mp3", "ogg", 
            "opus", "vorbis", "wav"
        ];

        public RecodeViewModel()
        {
            SelectedFormat = Formats.FirstOrDefault();
        }

        public string[] Formats => FORMATS;

        private bool _needRecode;
        public bool NeedRecode
        {
            get => _needRecode;
            set => SetProperty(ref _needRecode, value);
        }

        private string _selectedFormat;
        public string SelectedFormat 
        {
            get => _selectedFormat; 
            set => SetProperty(ref _selectedFormat, value); 
        }

        public string ToArgs()
            => NeedRecode ? $" --recode-video {SelectedFormat}" : String.Empty;
    }
}
