using Microsoft.Extensions.DependencyInjection;

namespace kc_yt_downloader.GUI.Model
{
    public interface IServicesConfigurator
    {
        void ConfigureServices(ServiceCollection services);
    }
}
