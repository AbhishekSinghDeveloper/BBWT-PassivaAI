using Autofac;
using BBF.Reporting.Core;
using BBF.Reporting.Widget.ControlSet.DbModel;
using BBF.Reporting.Widget.ControlSet.Interfaces;
using BBF.Reporting.Widget.ControlSet.Services;
using BBWM.Core.Autofac;
using BBWM.Core.ModuleLinker;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using BBF.Reporting.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BBF.Reporting.Widget.ControlSet;

public class WidgetControlSetModuleLinkage :
    IConfigureModuleLinkage,
    IDbModelCreateModuleLinkage,
    IDependenciesModuleLinkage
{
    public void ConfigureModule(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();

        const string sourceType = WidgetControlSetProvider.SourceType;
        var widgetProviderFactory = scope.ServiceProvider.GetService<IWidgetProviderFactory>();
        widgetProviderFactory!.RegisterWidgetProvider<IWidgetControlSetProvider>(sourceType);
    }

    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.RegisterReportingTable<WidgetControlSet>();

        var csiBuilder = builder.RegisterReportingTable<WidgetControlSetItem>();
        csiBuilder.Property(x => x.Name).HasMaxLength(500).IsRequired();
        csiBuilder.Property(x => x.HintText).HasMaxLength(500).IsRequired();
        csiBuilder.Property(x => x.ExtraSettings).HasColumnType("text");

        // Registering models configurations of the module for the main project's DB context
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IWidgetControlSetProvider, WidgetControlSetProvider>();
        builder.RegisterService<IWidgetControlSetViewService, WidgetControlSetViewService>();
        builder.RegisterService<IWidgetControlSetDataService, WidgetControlSetDataService>();
        builder.RegisterService<IWidgetControlSetBuilderService, WidgetControlSetBuilderService>();
    }
}