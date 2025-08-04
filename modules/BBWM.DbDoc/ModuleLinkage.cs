using Autofac;
using BBWM.Core.Autofac;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;
using BBWM.Core.ModelHashing;
using BBWM.Core.ModuleLinker;
using BBWM.DbDoc.DbMacros;
using BBWM.DbDoc.DbSchemas;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;
using BBWM.DbDoc.Services;
using BBWM.Menu;
using BBWM.Menu.DTO;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BBWM.DbDoc;

public class ModuleLinkage :
    IRouteRolesModuleLinkage,
    IInitialDataModuleLinkage,
    IMenuModuleLinkage,
    IServicesModuleLinkage,
    IDependenciesModuleLinkage,
    IConfigureModuleLinkage,
    IDbModelCreateModuleLinkage
{
    private const string DbDocSection = "DbDocSettings";

    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope) =>
        new() {
                new PageInfoDTO(Routes.DbExplorer, BBWM.Core.Roles.SuperAdminRole),
                new PageInfoDTO(Routes.ColumnTypes, BBWM.Core.Roles.SuperAdminRole),
                new PageInfoDTO(Routes.AddColumnType, BBWM.Core.Roles.SuperAdminRole),
                new PageInfoDTO(Routes.EditColumnType, BBWM.Core.Roles.SuperAdminRole)
        };

    public Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
        => serviceScope.ServiceProvider.GetService<IDbDocSyncService>().Synchronize();

    public void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus)
    {
        rootMenus.TechnicalAdmin.Children.Add(new MenuDTO(Routes.DbExplorer, "folder_open"));
        rootMenus.TechnicalAdmin.Children.Add(new MenuDTO(Routes.ColumnTypes, "view_column"));
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDbContextProvider, DbContextProvider>();

        var section = configuration.GetSection(DbDocSection);
        if (section.Get<DbDocSettings>() == null)
            throw new EmptyConfigurationSectionException(DbDocSection);
        services.Configure<DbDocSettings>(section);
    }

    public void RegisterDependencies(ContainerBuilder builder) =>
        builder
            .RegisterService<IConnectedDbService, ConnectedDbService>()
            .RegisterService<IDbSchemaManager, DbSchemaManager>(ServiceLifetime.Singleton)
            .RegisterService<IDbDocGitLabService, DbDocGitLabService>()
            .RegisterService<IColumnTypeService, ColumnTypeService>()
            .RegisterService<IDbDocPagedGridService, DbDocPagedGridService>()
            .RegisterService<IDbDocService, DbDocService>()
            .RegisterService<IDbDocSyncService, DbDocSyncService>()
            .RegisterService<IDbPathMacroService, DbPathMacroService>();

    public void ConfigureModule(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var provider = serviceScope.ServiceProvider.GetService<IDbContextProvider>();
        provider.Register(typeof(IDbContext));
        var dataLinkers = ModuleLinker.GetInstances<IDataContextModuleLinkage>();
        dataLinkers.ForEach(o => provider.Register(o.GetPrivateDataContextType()));

        serviceScope.ServiceProvider.GetService<IModelHashingService>()
            .IgnoreModelHashing<ColumnTypeDTO>()
            .IgnoreModelHashing<TableMetadataDTO>()
            .IgnoreModelHashing<ColumnMetadataDTO>()
            .IgnoreModelHashing<ColumnValidationMetadataDTO>()
            .IgnoreModelHashing<ColumnViewMetadataDTO>()
            .IgnoreModelHashing<ColumnViewMetadataDTO>()
            .IgnoreModelHashing<GridColumnViewDTO>();
    }

    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.Entity<ColumnType>();
        builder.Entity<ColumnMetadata>();
        builder.Entity<ColumnValidationMetadata>();
        builder.Entity<ColumnViewMetadata>();
        builder.Entity<DatabaseSource>();
        builder.Entity<Folder>();
        builder.Entity<GridColumnView>();
        builder.Entity<TableMetadata>();

        // Registering models configurations of the module for the main project's DB context
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
