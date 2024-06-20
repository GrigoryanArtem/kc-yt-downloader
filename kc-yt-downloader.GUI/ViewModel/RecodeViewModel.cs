using kc_yt_downloader.Model;
using NavigationMVVM;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class RecodeViewModel : ObservableDisposableObject
    {
        public RecodeViewModel()
        {
            SelectedFormat = Formats.FirstOrDefault();
        }

        public string[] Formats => Recode.FORMATS;

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

        public Recode? GetRecode()
            => NeedRecode && SelectedFormat is not null ? new Recode() { Format = SelectedFormat } : null;
    }
}
