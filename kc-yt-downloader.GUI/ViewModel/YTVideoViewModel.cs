using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.GUI.Model.Messages;
using kc_yt_downloader.Model;
using NavigationMVVM;
using NavigationMVVM.Services;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.ViewModel
{
    public partial class YTVideoViewModel : ObservableDisposableObject
    {
        private YtDlp _ytDlp;

        public YTVideoViewModel(YtDlp ytDlp, Video video, ParameterNavigationService<CutViewModelParameters, CutViewModel> cutNavigation, 
            NavigationService<ObservableDisposableObject> backNavigation, NavigationService<ObservableDisposableObject> dashboardNavigation)
        {
            _ytDlp = ytDlp;

            Video = video;
            var videoInfo = Video?.Info;
            ThumbnailUrl = videoInfo?.Thumbnails
                .OrderByDescending(t => t.Width.HasValue ? 1d - (400d / t.Width) : Double.NegativeInfinity)
                .ThenByDescending(t => t.Preference)
                .FirstOrDefault()?.Url;

            DurationString = GetDurationString(videoInfo?.Duration);

            if (videoInfo?.UploadDate is not null)
                UploadDate = DateTime.ParseExact(videoInfo?.UploadDate, "yyyyMMdd", CultureInfo.InvariantCulture);

            CutCommand = new RelayCommand(() => cutNavigation.Navigate(new() 
            { 
                VideoInfo = videoInfo, 
                BackNavigation = backNavigation,
                DashboardNavigation = dashboardNavigation
            }));

            OpenCommand = new RelayCommand(OnOpen);
        }

        public Video? Video { get; }
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
        public ICommand OpenCommand { get; }

        public void OnOpen()
        {
            if (String.IsNullOrEmpty(Video?.Info?.OriginalUrl))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = Video.Info.OriginalUrl,
                UseShellExecute = true
            });
        }

        [RelayCommand]
        public void DeleteVideo()
        {
            _ytDlp.DeleteVideo(Video);

            WeakReferenceMessenger.Default.Send(new VideosUpdatedMessage());
        }
    }
}
