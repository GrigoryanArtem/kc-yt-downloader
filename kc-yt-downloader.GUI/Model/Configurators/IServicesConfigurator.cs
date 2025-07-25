using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace kc_yt_downloader.GUI.Model.Configurators;

public interface IServicesConfigurator
{
    void ConfigureServices(HostBuilderContext context, IServiceCollection services);
}
