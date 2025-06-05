using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class RecodeViewModel : ObservableObject
    {
        private static SelectedSettings Settings => YtConfig.Global.SelectedSettings;
        public string[] Formats => Recode.FORMATS;

        public bool NeedRecode
        {
            get => Settings.NeedRecode;
            set => SetProperty(Settings.NeedRecode, value, nv => Settings.NeedRecode = nv);
        }

        public string SelectedFormat
        {
            get => Settings.RecodeFormat;
            set => SetProperty(Settings.RecodeFormat, value, nv => Settings.RecodeFormat = nv);
        }

        public Recode? GetRecode()
            => NeedRecode && SelectedFormat is not null ? new Recode() { Format = SelectedFormat } : null;
    }
}
