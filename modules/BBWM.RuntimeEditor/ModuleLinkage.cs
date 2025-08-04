using BBWM.Core.Exceptions;
using BBWM.Core.ModuleLinker;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.RuntimeEditor;

public class ModuleLinkage : IServicesModuleLinkage
{
    private readonly string sectionName = "RuntimeEditor";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var sectionRte = configuration.GetSection(sectionName);
        if (sectionRte.Get<RuntimeEditorSettings>() is null)
            throw new EmptyConfigurationSectionException(sectionName);
        services.Configure<RuntimeEditorSettings>(sectionRte);
    }
}
