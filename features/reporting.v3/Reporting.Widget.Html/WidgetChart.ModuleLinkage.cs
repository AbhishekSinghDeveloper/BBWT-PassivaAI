using Autofac;
using BBF.Reporting.Core;
using BBF.Reporting.Widget.Html.DbModel;
using BBF.Reporting.Widget.Html.Interfaces;
using BBF.Reporting.Widget.Html.Services;
using BBWM.Core.Autofac;
using BBWM.Core.ModuleLinker;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using BBF.Reporting.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BBF.Reporting.Widget.Html;

public class WidgetChartModuleLinkage :
    IConfigureModuleLinkage,
    IDbModelCreateModuleLinkage,
    IDependenciesModuleLinkage
{
    public void ConfigureModule(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();

        const string sourceType = WidgetHtmlProvider.SourceType;
        var widgetProviderFactory = scope.ServiceProvider.GetService<IWidgetProviderFactory>();
        widgetProviderFactory!.RegisterWidgetProvider<IWidgetHtmlProvider>(sourceType);
    }

    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.RegisterReportingTable<WidgetHtml>();

        // Registering models configurations of the module for the main project's DB context
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IWidgetHtmlProvider, WidgetHtmlProvider>();
        builder.RegisterService<IWidgetHtmlViewService, WidgetHtmlViewService>();
        builder.RegisterService<IWidgetHtmlBuilderService, WidgetHtmlBuilderService>();
    }
}