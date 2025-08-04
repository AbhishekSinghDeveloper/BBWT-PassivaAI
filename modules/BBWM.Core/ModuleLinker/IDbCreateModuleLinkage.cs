using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core.ModuleLinker;

public interface IDbCreateModuleLinkage
{
    void Create(IServiceScope serviceScope);
}

