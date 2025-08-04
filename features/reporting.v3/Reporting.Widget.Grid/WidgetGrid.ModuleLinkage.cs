using Autofac;
using BBF.Reporting.Core;
using BBF.Reporting.Widget.Grid.DbModel;
using BBF.Reporting.Widget.Grid.Interfaces;
using BBF.Reporting.Widget.Grid.Services;
using BBWM.Core.Autofac;
using BBWM.Core.ModuleLinker;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using BBF.Reporting.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BBF.Reporting.Widget.Grid;

public class WidgetGridModuleLinkage :
    IConfigureModuleLinkage,
    IDbModelCreateModuleLinkage,
    IDependenciesModuleLinkage
{
    public void ConfigureModule(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();

        const string sourceType = WidgetGridProvider.SourceType;
        var widgetProviderFactory = scope.ServiceProvider.GetService<IWidgetProviderFactory>();
        widgetProviderFactory!.RegisterWidgetProvider<IWidgetGridProvider>(sourceType);
    }

    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.RegisterReportingTable<WidgetGrid>();

        var colBuilder = builder.RegisterReportingTable<WidgetGridColumn>();
        colBuilder.Property(x => x.QueryAlias).HasMaxLength(256);
        colBuilder.Property(x => x.Header).HasMaxLength(500);
        colBuilder.Property(x => x.ExtraSettings).HasColumnType("text");
        colBuilder.Property(x => x.Footer).HasColumnType("text");


        // Registering models configurations of the module for the main project's DB context
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IWidgetGridProvider, WidgetGridProvider>();
        builder.RegisterService<IWidgetGridViewService, WidgetGridViewService>();
        builder.RegisterService<IWidgetGridDataService, WidgetGridDataService>();
        builder.RegisterService<IWidgetGridBuilderService, WidgetGridBuilderService>();
    }
}