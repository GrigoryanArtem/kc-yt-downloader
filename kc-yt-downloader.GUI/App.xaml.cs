using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.GUI.ViewModel;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NavigationMVVM.Services;
using NavigationMVVM.Stores;
using NLog.Extensions.Hosting;
using System.Windows;

namespace kc_yt_downloader.GUI;

public partial class App : Application
{
    private readonly IHost _host;

    private Task _apiTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public App()
    {
        var services = new ServiceCollection();
        var configurator = new DefaultServiceConfigurator();

        var builder = Host.CreateDefaultBuilder();
        _host = builder
            .UseNLog()
            .ConfigureServices(configurator.ConfigureServices)
            .Build();
    }

    public new static App Current => (App)Application.Current;
    public IServiceProvider Services => _host.Services;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var initialNavigationService = Services.GetRequiredService<NavigationService<UpdateViewModel>>();
        initialNavigationService.Navigate();

        var mainWindow = Services.GetService<MainWindow>();
        mainWindow!.Show();

        var listener = new CutTaskListener();
        _apiTask = Task.Run(() => listener.Listen(Handle, _cancellationTokenSource.Token));
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

    protected async override void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }

        _cancellationTokenSource.Cancel();
        await _apiTask;

        base.OnExit(e);
    }
}
