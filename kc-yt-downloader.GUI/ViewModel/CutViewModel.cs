using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;
using NavigationMVVM;
using NavigationMVVM.Commands;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class CutViewModel : ObservableDisposableObject
    {
        private const string HTTPS_PROTOCOL = "https";
        private const string NONE = "none";

        private readonly VideoInfo _info;

        public CutViewModel(CutViewModelParameters parameters)
        {
            _info = parameters.VideoInfo;

            var httpsFormats = _info.Formats
                .Where(f => f.Protocol == HTTPS_PROTOCOL)
                .OrderByDescending(f => f.Quality ?? 0)
                .ToArray();

            VideoFormats = httpsFormats.Where(f => f.VCodec != NONE && f.ACodec == NONE).Select(f => new VideoFormatViewModel(f)).ToArray();
            AudioFormats = httpsFormats.Where(f => f.ACodec != NONE && f.VCodec == NONE).Select(f => new VideoFormatViewModel(f)).ToArray();

            VideoFormats[0].IsSelected = true;

            BackCommand = new NavigateCommand(parameters.BackNavigation);
        }

        public string Title => _info.Title;

        public VideoFormatViewModel[] VideoFormats { get; }
        public VideoFormatViewModel[] AudioFormats { get; }

        public ICommand BackCommand { get; }
    }
}
