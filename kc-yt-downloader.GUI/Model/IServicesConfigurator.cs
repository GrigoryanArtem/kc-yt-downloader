using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace kc_yt_downloader.GUI.Model
{
    public interface IServicesConfigurator
    {
        void ConfigureServices(HostBuilderContext context, IServiceCollection services);
    }
}
