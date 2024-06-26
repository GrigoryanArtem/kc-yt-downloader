﻿using kc_yt_downloader.GUI.ViewModel;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using NavigationMVVM.Services;
using NavigationMVVM.Stores;

namespace kc_yt_downloader.GUI.Model
{
    public class DefaultServiceConfigurator : IServicesConfigurator
    {
        public void ConfigureServices(ServiceCollection services)
        {
            ConfigureViewModels(services);
            services.AddSingleton(s => new MainWindow
            {
                DataContext = s.GetRequiredService<MainWindowViewModel>()
            });
        }

        private static void ConfigureViewModels(ServiceCollection services)
        {            
            services.AddSingleton(s => 
            {
                var ytDlp = new YtDlp(YtConfig.Global.CacheDirectory);
                ytDlp.Open();

                return ytDlp;
            });

            services.AddSingleton<NavigationStore>();
            services.AddSingleton<ModalNavigationStore>();
            services.AddSingleton<CloseModalNavigationService>();


            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton(s => new NavigationService<DashboardViewModel>
            (
                s.GetRequiredService<NavigationStore>(),
                () => s.GetRequiredService<DashboardViewModel>()
            ));

            
            services.AddSingleton<MainWindowViewModel>();
        }
    }
}
