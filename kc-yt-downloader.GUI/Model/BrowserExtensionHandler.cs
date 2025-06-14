using kc_yt_downloader.GUI.ViewModel;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using NavigationMVVM.Services;
using NavigationMVVM.Stores;

namespace kc_yt_downloader.GUI.Model;

public class BrowserExtensionHandler
{
    private const string PREFIX = "http://localhost:5000/api/cut/";

    private Task _apiTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public BrowserExtensionHandler()
    {
        
    }

    public void Run()
    {        
        _apiTask = Task.Run(() => CutTaskListener.Listen(PREFIX, Handle, _cancellationTokenSource.Token));
    }

    public async Task Stop()
    {
        _cancellationTokenSource.Cancel();
        await _apiTask;
    }

    private void Handle(CutTaskRequest request)
    {
        var services = App.Current.Services;
        var ytDlp = services.GetRequiredService<YtDlp>();

        var cutViewLoadingViewModel = new CutViewLoadingViewModel(() => Task.Run(() =>
        {
            var video = ytDlp.GetVideoByUrl(request.VideoId);

            return new CutViewModelParameters()
            {
                Video = video,
                Source = new()
                {
                    TimeRange = new()
                    {
                        From = TimeSpan.FromSeconds(request.Start).ToString("hh\\:mm\\:ss"),
                        To = TimeSpan.FromSeconds(request.End).ToString("hh\\:mm\\:ss")
                    }
                }
            };
        }));


        var store = services.GetRequiredService<NavigationStore>();
        var navigation = new NavigationService<CutViewLoadingViewModel>(store, () => cutViewLoadingViewModel);
        navigation.Navigate();
    }
}
