using Autofac;
using BBWM.Core.Autofac;
using BBWM.Core.ModuleLinker;

namespace BBWM.FormIO.Connectors.ReportingV3;

public class Reporting3ConnectorModuleLinkage : IDependenciesModuleLinkage
{
    public void RegisterDependencies(ContainerBuilder builder) =>
        builder.RegisterService<IQueryableFormsService, QueryableFormsService>();
}
