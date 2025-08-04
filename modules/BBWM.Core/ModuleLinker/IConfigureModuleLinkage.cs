using Microsoft.AspNetCore.Builder;

namespace BBWM.Core.ModuleLinker;

public interface IConfigureModuleLinkage
{
    void ConfigureModule(IApplicationBuilder app);
}
