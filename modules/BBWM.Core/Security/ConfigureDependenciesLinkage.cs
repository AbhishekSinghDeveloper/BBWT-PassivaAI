using BBWM.Core.ModuleLinker;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core.Security;

public class ConfigureDependenciesLinkage : IServicesModuleLinkage
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ContentSecurityPolicyOptions>(
            configuration.GetSection(ContentSecurityPolicyOptions.SectionName));
    }
}
