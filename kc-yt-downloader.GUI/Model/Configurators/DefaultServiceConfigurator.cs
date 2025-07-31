using kc_yt_downloader.GUI.ViewModel;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NavigationMVVM.Services;
using NavigationMVVM.Stores;

namespace kc_yt_downloader.GUI.Model.Configurators;

public class DefaultServiceConfigurator : IServicesConfigurator
{
    public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        ConfigureViewModels(services);
        services.AddSingleton(s => new MainWindow
        {
            DataContext = s.GetRequiredService<MainWindowViewModel>()
        });

        services.AddSingleton<BrowserExtensionHandler>();
        services.AddSingleton<GlobalErrorHandler>();
    }

    private static void ConfigureViewModels(IServiceCollection services)
    {
        var ytDlp = new YtDlp(YtConfig.Global.DataDirectory);
        ytDlp.Open();

        services.AddSingleton<FFmpeg>();
        services.AddSingleton(ytDlp);
        services.AddSingleton<YtDlpProxy>(_ => new(ytDlp));
        services.AddSingleton<TasksFactory>();

        services.AddSingleton<NavigationStore>();
        services.AddSingleton<ModalNavigationStore>();
        services.AddSingleton<CloseModalNavigationService>();

        services.AddTransient<DraftsListViewModel>();

        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton(s => new NavigationService<DashboardViewModel>
        (
            s.GetRequiredService<NavigationStore>(),
            () => s.GetRequiredService<DashboardViewModel>()
        ));

        services.AddSingleton<UpdateViewModel>();
        services.AddSingleton(s => new NavigationService<UpdateViewModel>
        (
            s.GetRequiredService<NavigationStore>(),
            () => s.GetRequiredService<UpdateViewModel>()
        ));

        services.AddTransient(s => new ParameterNavigationService<string, ErrorInformationViewModel>
        (
            s.GetRequiredService<ModalNavigationStore>(),
            args => new(args)
        ));

        services.AddTransient(s => new ParameterNavigationService<VideoPreview, VideoInfoControlViewModel>
        (
            s.GetRequiredService<ModalNavigationStore>(),
            args => new(args)
        ));

        services.AddSingleton<MainWindowViewModel>();
    }
}
