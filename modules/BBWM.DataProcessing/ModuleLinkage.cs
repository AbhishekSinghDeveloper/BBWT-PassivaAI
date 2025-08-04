using Autofac;

using BBWM.Core.Autofac;
using BBWM.Core.ModuleLinker;
using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.FileReaders;
using BBWM.DataProcessing.Services;
using BBWM.DataProcessing.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace BBWM.DataProcessing;

public class ModuleLinkage : IDependenciesModuleLinkage, ISignalRModuleLinkage
{
    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IDataImportReaderProvider, DataImportReaderProvider>();
        builder.RegisterService<ITypeValidatorsProvider, TypeValidatorsProvider>();
        builder.RegisterService<IDataImportHelper, DataImportHelper>();
    }

    void ISignalRModuleLinkage.MapHubs(IEndpointRouteBuilder routes) =>
        routes.MapHub<DataImportHub>("/api/import-processing");
}
