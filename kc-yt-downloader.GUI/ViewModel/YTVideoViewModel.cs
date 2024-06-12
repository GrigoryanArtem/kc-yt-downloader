using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;
using NavigationMVVM;
using NavigationMVVM.Services;
using System.Globalization;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class YTVideoViewModel : ObservableDisposableObject
    {
        public YTVideoViewModel(VideoInfo info, ParameterNavigationService<CutViewModelParameters, CutViewModel> cutNavigation, NavigationService<ObservableDisposableObject> backNavigation)
        {
            VideoInfo = info;
            ThumbnailUrl = VideoInfo?.Thumbnails
                .OrderByDescending(t => t.Width.HasValue ? 1d - (400d / t.Width) : Double.NegativeInfinity)
                .ThenByDescending(t => t.Preference)
                .FirstOrDefault()?.Url;

            DurationString = GetDurationString(VideoInfo?.Duration);

            if (VideoInfo?.UploadDate is not null)
                UploadDate = DateTime.ParseExact(VideoInfo?.UploadDate, "yyyyMMdd", CultureInfo.InvariantCulture);

            CutCommand = new RelayCommand(() => cutNavigation.Navigate(new() { VideoInfo = VideoInfo, BackNavigation = backNavigation }));
        }

        public VideoInfo? VideoInfo { get; }
        public string? ThumbnailUrl { get; }
        public string DurationString { get; }
        public DateTime UploadDate { get; }

        private static string GetDurationString(int? sec)
        {
            if (!sec.HasValue)
                return "Unknown";

            var ts = TimeSpan.FromSeconds(sec.Value);
            return ts.ToString("c");
        }

        public ICommand CutCommand { get; }
    }
}
