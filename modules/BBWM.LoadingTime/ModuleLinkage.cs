using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;
using BBWM.Core.ModuleLinker;
using BBWM.Menu;
using BBWM.Menu.DTO;
using BBWM.SystemSettings;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.LoadingTime;

public class ModuleLinkage :
    IInitialDataModuleLinkage,
    IRouteRolesModuleLinkage,
    IMenuModuleLinkage,
    IConfigureModuleLinkage,
    IDbModelCreateModuleLinkage
{
    private readonly string settingsSection = "LoadingTimeSettings";

    public Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        var service = serviceScope.ServiceProvider.GetService<ISettingsService>();

        var appSettings = new[]
         {
                new SettingsDTO { Value = service.GetSettingsSection<LoadingTimeSettings>() ?? new LoadingTimeSettings() },
            };

        return service.Save(appSettings);
    }

    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope)
        => new List<PageInfoDTO>
        {
                 new PageInfoDTO(Routes.LoadingTime, Core.Roles.SuperAdminRole)
        };

    public void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus)
        => rootMenus.TechnicalAdmin.Children.Add(new MenuDTO(Routes.LoadingTime));

    public void ConfigureModule(IApplicationBuilder app) => app.RegisterSection<LoadingTimeSettings>(settingsSection);

    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.Entity<LoadingTime>();
    }
}
