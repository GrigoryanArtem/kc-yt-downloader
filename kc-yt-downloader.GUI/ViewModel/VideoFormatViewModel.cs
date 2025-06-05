using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class VideoFormatViewModel : ObservableObject
    {
        private readonly VideoFormat _format;

        public VideoFormatViewModel(VideoFormat format)
        {
            _format = format;
            DataTable = CreateTable();
        }

        public KeyValuePair<string, string>[] DataTable { get; }

        public string Id => _format.FormatId;
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
                new("Size", SizeConverter.ToFriendlyString(_format.FileSize, 2)),
                new("Approximate size", SizeConverter.ToFriendlyString(_format.FileSizeApprox, 2)),
                new("Extension", _format.Ext),
                new("Fps", _format.Fps?.ToString("f1")),
                new("Bitrate", _format.Tbr?.ToString("f3")),
                new("Resolution", _format.Resolution),
                new("Aspect ratio", _format.AspectRatio?.ToString("f1")),
            ];
        }


    }
}
