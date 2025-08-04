using BBWM.Core.ModuleLinker;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Demo.Data;

public class DbCreateModuleLinkage : IDbCreateModuleLinkage
{
    public void Create(IServiceScope serviceScope)
    {
        var context = serviceScope.ServiceProvider.GetService<IDemoDataContext>();

        context.Database.Migrate();
    }
}
