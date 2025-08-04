using Autofac;
using BBWM.Core.Autofac;
using BBWM.Core.ModuleLinker;
using BBWM.FormIO.Connectors.ReportingV2;

namespace BBWM.FormIO;

public class Reporting2ConnectorModuleLinkage : IDependenciesModuleLinkage
{
    public void RegisterDependencies(ContainerBuilder builder) =>
        builder.RegisterService<IFormsQueryableTablesProvider, FormsQueryableTableProvider>();
}
