using Autofac;

using BBWM.Core.Autofac;
using BBWM.Core.Data;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;
using BBWM.Core.ModuleLinker;
using BBWM.Core.Services;
using BBWM.Menu;
using BBWM.Menu.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core.Audit;

public class ModuleLinkage : IRouteRolesModuleLinkage, IMenuModuleLinkage, IDependenciesModuleLinkage, IDataContextModuleLinkage, IDbCreateModuleLinkage, IServicesModuleLinkage
{
    public IServiceCollection AddDataContext(IServiceCollection services, IConfiguration configuration, DatabaseConnectionSettings defaultConnectionSettings)
    {
        switch (defaultConnectionSettings.DatabaseType)
        {
            case DatabaseType.MsSql:
                services.AddDbContext<IAuditContext, AuditContext>(
                    defaultConnectionSettings.GetDbContextOptionsBuilder<AuditContext>(
                        configuration.GetConnectionString("AuditConnection")).Options);
                break;
            case DatabaseType.MySql:
                services.AddDbContext<IAuditContext, AuditContext>(
                    defaultConnectionSettings.GetDbContextOptionsBuilder<AuditContext>(
                        configuration.GetConnectionString("AuditMySqlConnection")).Options);
                break;
            default: throw new Exception($"Data base type '{defaultConnectionSettings.DatabaseType}' is not supported.");
        };

        return services;
    }

    public void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus) =>
        rootMenus.SecurityAdmin.Children.Add(new MenuDTO(Routes.DataAudit));

    public Type GetPrivateDataContextType() => typeof(IAuditContext);

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration) =>
        services.AddScoped(typeof(IAuditWrapper), typeof(AuditWrapper));

    public void Create(IServiceScope serviceScope)
    {
        var auditContext = serviceScope.ServiceProvider.GetService<IAuditContext>();

        auditContext.Database.EnsureCreated();
    }

    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope) => new()
    {
        new PageInfoDTO(Routes.DataAudit, Roles.SystemAdminRole)
    };

    public void RegisterDependencies(ContainerBuilder builder) =>
        builder.RegisterService<IDataService<IAuditContext>, DataService<IAuditContext>>();
}
