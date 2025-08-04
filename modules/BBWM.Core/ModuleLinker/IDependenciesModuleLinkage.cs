using Autofac;

namespace BBWM.Core.ModuleLinker;

public interface IDependenciesModuleLinkage
{
    void RegisterDependencies(ContainerBuilder builder);
}
