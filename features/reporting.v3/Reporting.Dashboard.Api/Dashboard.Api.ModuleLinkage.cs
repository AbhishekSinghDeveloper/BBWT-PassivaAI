using Autofac;
using BBWM.Core.Autofac;
using BBWM.Core.Membership;
using BBWM.Core.ModuleLinker;

namespace BBF.Reporting.Dashboard.Api;

public class ModuleLinkage : IDependenciesModuleLinkage
{
    public void RegisterDependencies(ContainerBuilder builder) =>
        builder.RegisterService<IRouteRolesModule, RouteRolesModule>();
}
