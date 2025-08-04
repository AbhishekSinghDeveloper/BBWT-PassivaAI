using Autofac;
using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Core.Services;
using BBWM.Core.Autofac;
using BBWM.Core.ModuleLinker;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using BBF.Reporting.Core.Enums;
using BBF.Reporting.Core.ModelBinders;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BBF.Reporting.Core;

public class ReportingCoreModuleLinkage :
    IDbModelCreateModuleLinkage,
    IDependenciesModuleLinkage,
    IConfigureMvcModuleLinkage
{
    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        var qBuilder = builder.RegisterReportingTable<QuerySource>();
        qBuilder.Property(source => source.Id).ValueGeneratedOnAdd();
        qBuilder.Property(source => source.QueryType).HasMaxLength(50).IsRequired();
        qBuilder.Property(source => source.Name).HasMaxLength(500).IsRequired(false);
        qBuilder.Property(source => source.CreatedOn).HasDefaultValue(new DateTime(2024, 7, 18));
        qBuilder.Property(source => source.FilterMode).HasDefaultValue(QueryFilterMode.UserOrganizationsFilter);
        qBuilder.HasMany(source => source.Organizations).WithMany().UsingEntity("RbQuerySourceOrganization");
        qBuilder.HasOne(source => source.ReleaseQuery).WithMany().HasForeignKey(source => source.ReleaseQueryId).OnDelete(DeleteBehavior.NoAction);

        var wBuilder = builder.RegisterReportingTable<WidgetSource>();
        wBuilder.Property(source => source.Id).ValueGeneratedOnAdd();
        wBuilder.Property(source => source.WidgetType).HasMaxLength(50).IsRequired();
        wBuilder.Property(source => source.Name).HasMaxLength(500).IsRequired(false);
        wBuilder.Property(source => source.Title).HasMaxLength(500).IsRequired(false);
        wBuilder.Property(source => source.Code).HasMaxLength(200).IsRequired(false);
        wBuilder.Property(source => source.CreatedOn).HasDefaultValue(new DateTime(2024, 7, 18));
        wBuilder.HasMany(source => source.Organizations).WithMany().UsingEntity("RbWidgetSourceOrganization");
        wBuilder.HasOne(source => source.ReleaseWidget).WithMany().HasForeignKey(source => source.ReleaseWidgetId).OnDelete(DeleteBehavior.NoAction);

        var vBuilder = builder.RegisterReportingTable<Variable>();
        wBuilder.Property(source => source.Id).ValueGeneratedOnAdd();
        vBuilder.Property(variable => variable.Name).HasMaxLength(128).IsRequired();

        var vrBuilder = builder.RegisterReportingTable<VariableRule>();
        vrBuilder.Property(rule => rule.VariableName).HasMaxLength(128).IsRequired();
        vrBuilder.Property(rule => rule.Operand).HasMaxLength(128);

        var frBuilder = builder.RegisterReportingTable<FilterRule>();
        frBuilder.Property(rule => rule.Operand).HasMaxLength(128);

        // Registering models configurations of the module for the main project's DB context
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<ILoggedUserService, LoggedUserService>();
        builder.RegisterService<IQueryProviderFactory, QueryProviderFactory>();
        builder.RegisterService<IWidgetProviderFactory, WidgetProviderFactory>();
        builder.RegisterService<IWidgetSourceService, WidgetSourceService>();
        builder.RegisterService<INamedQuerySourceService, NamedQuerySourceService>();
        builder.RegisterService<IContextVariableService, ContextVariableService>();
        builder.RegisterService<IVariablesService, VariablesService>();
    }

    public IEnumerable<IModelBinderProvider> GetModelBinderProviders()
        => new List<IModelBinderProvider> { new EmittedVariableModelBinderProvider() };
}