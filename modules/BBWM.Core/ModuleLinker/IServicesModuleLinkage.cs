using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core.ModuleLinker;

public interface IServicesModuleLinkage
{
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
