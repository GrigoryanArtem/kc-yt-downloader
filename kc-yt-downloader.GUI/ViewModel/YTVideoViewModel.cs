using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using NavigationMVVM.Services;
using NavigationMVVM.Stores;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class YTVideoViewModel : ObservableObject
{
    private readonly YtDlp _ytDlp;    

    public YTVideoViewModel(
        YtDlp ytDlp,
        VideoPreview video)
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

        var cutViewLoadingViewModel = new CutViewLoadingViewModel(() => Task.Run(() =>
        {
            {
                var video = _ytDlp.GetVideoByUrl(Video.Info.OriginalUrl);

                return new CutViewModelParameters()
                {
                    Video = video
                };
            }
        }));

        var services = App.Current.Services;
        var store = services.GetRequiredService<NavigationStore>();
        var navigation = new NavigationService<CutViewLoadingViewModel>(store, () => cutViewLoadingViewModel);        

        CutCommand = new RelayCommand(navigation.Navigate);
        OpenCommand = new RelayCommand(OnOpen);
    }

    public VideoPreview? Video { get; }
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
        var services = App.Current.Services;    
        var navigation = services.GetRequiredService<ParameterNavigationService<VideoPreview, VideoInfoControlViewModel>>();

        navigation.Navigate(Video);
    }         
}
