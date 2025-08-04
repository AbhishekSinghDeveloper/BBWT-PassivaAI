using Autofac;
using BBF.Reporting.Core;
using BBF.Reporting.Widget.Chart.DbModel;
using BBF.Reporting.Widget.Chart.Interfaces;
using BBF.Reporting.Widget.Chart.Services;
using BBWM.Core.Autofac;
using BBWM.Core.ModuleLinker;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using BBF.Reporting.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BBF.Reporting.Widget.Chart;

public class WidgetChartModuleLinkage :
    IConfigureModuleLinkage,
    IDbModelCreateModuleLinkage,
    IDependenciesModuleLinkage
{
    public void ConfigureModule(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();

        const string sourceType = WidgetChartProvider.SourceType;
        var widgetProviderFactory = scope.ServiceProvider.GetService<IWidgetProviderFactory>();
        widgetProviderFactory!.RegisterWidgetProvider<IWidgetChartProvider>(sourceType);
    }

    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.RegisterReportingTable<WidgetChart>();

        var colBuilder = builder.RegisterReportingTable<WidgetChartColumn>();
        colBuilder.Property(column => column.QueryAlias).HasMaxLength(256);
        colBuilder.Property(column => column.ChartAlias).HasMaxLength(256);

        // Registering models configurations of the module for the main project's DB context
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IWidgetChartProvider, WidgetChartProvider>();
        builder.RegisterService<IWidgetChartViewService, WidgetChartViewService>();
        builder.RegisterService<IWidgetChartDataService, WidgetChartDataService>();
        builder.RegisterService<IWidgetChartBuilderService, WidgetChartBuilderService>();
    }
}