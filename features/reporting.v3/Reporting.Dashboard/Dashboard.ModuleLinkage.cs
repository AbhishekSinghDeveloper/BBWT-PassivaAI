using Autofac;
using BBF.Reporting.Core;
using BBF.Reporting.Dashboard.DbModel;
using BBF.Reporting.Dashboard.Interfaces;
using BBF.Reporting.Dashboard.Services;
using BBWM.Core.Autofac;
using BBWM.Core.ModuleLinker;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using BBF.Reporting.Dashboard.Enums;

namespace BBF.Reporting.Dashboard;

public class DashboardModuleLinkage :
    IDbModelCreateModuleLinkage,
    IDependenciesModuleLinkage
{
    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        var dBuilder = builder.RegisterReportingTable<DbModel.Dashboard>();
        dBuilder.Property(dashboard => dashboard.Id).ValueGeneratedOnAdd();
        dBuilder.Property(dashboard => dashboard.Name).HasMaxLength(500).IsRequired();
        dBuilder.Property(dashboard => dashboard.Layout).HasDefaultValue(LayoutType.Dividers).IsRequired();
        dBuilder.Property(dashboard => dashboard.WidgetsMargin).HasDefaultValue(20).IsRequired();
        dBuilder.Property(dashboard => dashboard.WidgetsPadding).HasDefaultValue(15).IsRequired();
        dBuilder.Property(dashboard => dashboard.CreatedOn).HasDefaultValue(new DateTime(2024, 6, 27));
        dBuilder.HasMany(dashboard => dashboard.Organizations).WithMany().UsingEntity("RbDashboardOrganization");

        builder.RegisterReportingTable<DashboardWidget>();

        // Registering models configurations of the module for the main project's DB context
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IDashboardViewService, DashboardViewService>();
        builder.RegisterService<IDashboardBuilderService, DashboardBuilderService>();
    }
}