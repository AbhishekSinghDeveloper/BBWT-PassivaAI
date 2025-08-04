using BBWM.Core.ModuleLinker;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.ReportProblem;

public class ServicesModuleLinkage : IServicesModuleLinkage
{
    private const string supportSection = "SupportSettings";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(supportSection);
        if (section.Get<SupportSettings>() is null)
            // TODO: should be EmptyConfigurationSectionException. Resolve dependencies
            throw new Exception(supportSection);
        services.Configure<SupportSettings>(section);
    }
}
