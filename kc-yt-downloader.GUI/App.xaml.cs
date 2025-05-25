using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.GUI.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NavigationMVVM.Services;
using System.Windows;

namespace kc_yt_downloader.GUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;        

    public App()
    {   
        var configurator = new DefaultServiceConfigurator();

        var builder = Host.CreateDefaultBuilder();
        _host = builder.ConfigureServices(configurator.ConfigureServices)
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
    }
    protected async override void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }

        base.OnExit(e);
    }
}
