using kc_yt_downloader.GUI.ViewModel;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NavigationMVVM.Services;
using NavigationMVVM.Stores;

namespace kc_yt_downloader.GUI.Model
{
    public class DefaultServiceConfigurator : IServicesConfigurator
    {
        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            ConfigureViewModels(services);
            services.AddSingleton(s => new MainWindow
            {
                DataContext = s.GetRequiredService<MainWindowViewModel>()
            });
        }

        private static void ConfigureViewModels(IServiceCollection services)
        {
            var ytDlp = new YtDlp(YtConfig.Global.CacheDirectory);
            ytDlp.Open();

            services.AddSingleton(ytDlp);

            services.AddSingleton<NavigationStore>();
            services.AddSingleton<ModalNavigationStore>();
            services.AddSingleton<CloseModalNavigationService>();


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

            services.AddSingleton<MainWindowViewModel>();
        }
    }
}
