using kc_yt_downloader.Model;
using NavigationMVVM;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class VideoFormatViewModel : ObservableDisposableObject
    {
        static readonly string[] SIZE_SUFFIXES = [ "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" ];

        private VideoFormat _format;

        public VideoFormatViewModel(VideoFormat format)
        {
            _format = format;
            DataTable = CreateTable();
        }

        public KeyValuePair<string, string>[] DataTable { get; }

        public string Format => _format.Format;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; 
            set => SetProperty(ref _isSelected, value);
        }

        private KeyValuePair<string, string?>[] CreateTable()
        {
            return 
            [
                new("Id", _format.FormatId),
                new("Protocol", _format.Protocol),
                new("Size", FormatSize(_format.FileSize, 2)),                
                new("Approximate size", FormatSize(_format.FileSizeApprox, 2)),
                new("Extension", _format.Ext),
                new("Fps", _format.Fps?.ToString("f1")),
                new("Bitrate", _format.Tbr?.ToString("f3")),
                new("Resolution", _format.Resolution),
                new("Aspect ratio", _format.AspectRatio?.ToString("f1")),
            ];
        }

        private static string FormatSize(long? value, int decimalPlaces = 1)
        {
            if (!value.HasValue)
                return String.Empty;

            if (value < 0) 
                return "-" + FormatSize(-value, decimalPlaces);

            int i = 0;
            decimal dValue = (decimal)value;
            while (Math.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SIZE_SUFFIXES[i]);
        }
    }
}
