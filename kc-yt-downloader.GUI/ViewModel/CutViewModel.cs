using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;
using NavigationMVVM;
using NavigationMVVM.Commands;
using System.Windows;
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

            var formats = _info.FormatId.Split("+", StringSplitOptions.RemoveEmptyEntries);
            var (vf, af) = (formats[0], formats[1]);

            var httpsFormats = _info.Formats
                .Where(f => f.Protocol == HTTPS_PROTOCOL)
                .OrderByDescending(f => f.Quality ?? 0)
                .ToArray();

            VideoFormatsSelector = new(httpsFormats.Where(f => f.VCodec != NONE && f.ACodec == NONE)
                .Select(f => new VideoFormatViewModel(f))
                .ToArray(), vf);

            AudioFormatsSelector = new(httpsFormats.Where(f => f.ACodec != NONE && f.VCodec == NONE)
                .Select(f => new VideoFormatViewModel(f))
                .ToArray(), af);

            TimeRange = new(_info.DurationString);

            BackCommand = new NavigateCommand(parameters.BackNavigation);
            AddToQueueCommand = new RelayCommand(OnAddToQueueCommand);
        }

        public string Title => _info.Title;

        public VideFormatSelectorViewModel VideoFormatsSelector { get; }
        public VideFormatSelectorViewModel AudioFormatsSelector { get; }
        public RecodeViewModel Recode { get; } = new();

        public FileNameControlViewModel FileNameControl { get; } = new();
        public TimeRangeViewModel TimeRange { get; }

        public ICommand BackCommand { get; }
        public ICommand AddToQueueCommand { get; }

        private void OnAddToQueueCommand()
        {
            var args = $"-f \"{VideoFormatsSelector.SelectedFormat.Id}+{AudioFormatsSelector.SelectedFormat.Id}\"{TimeRange.ToArgs()}{Recode.ToArgs()}" +
                    $" \"https://www.youtube.com/live/NjbOiUBf938\" -o {FileNameControl.GetFullPath()}";
            MessageBox.Show(args);
        }
    }
}
