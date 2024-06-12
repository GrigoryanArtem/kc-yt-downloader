using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.GUI.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using NavigationMVVM.Services;
using System.Configuration;
using System.Data;
using System.Windows;

namespace kc_yt_downloader.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            IServicesConfigurator configurator = new DefaultServiceConfigurator();

            configurator.ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            INavigationService initialNavigationService = _serviceProvider.GetRequiredService<NavigationService<DashboardViewModel>>();
            initialNavigationService.Navigate();

            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}
