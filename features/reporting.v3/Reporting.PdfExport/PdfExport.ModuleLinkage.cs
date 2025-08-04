using Autofac;
using BBF.Reporting.PdfExport.Interfaces;
using BBF.Reporting.PdfExport.Services;
using BBWM.Core.Autofac;
using BBWM.Core.ModuleLinker;

namespace BBF.Reporting.PdfExport;

public class ModuleLinkage : IDependenciesModuleLinkage
{
    public void RegisterDependencies(ContainerBuilder builder) =>
        builder.RegisterService<IPdfExportService, PdfExportService>();
}