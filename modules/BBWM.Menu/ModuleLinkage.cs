using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;
using BBWM.Core.ModelHashing;
using BBWM.Core.ModuleLinker;
using BBWM.Menu.DTO;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Menu;

public class ModuleLinkage : IConfigureModuleLinkage, IRouteRolesModuleLinkage, IMenuModuleLinkage
{
    public void ConfigureModule(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var modelHashingService = serviceScope.ServiceProvider.GetService<IModelHashingService>();
        // Models hashing settings
        modelHashingService.IgnoreModelHashing<MenuDTO>();
        modelHashingService.IgnoreModelHashing<FooterMenuItemDTO>();
    }

    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope) =>
        new List<PageInfoDTO>
        {
                new PageInfoDTO(Routes.MenuDesigner, Core.Roles.SuperAdminRole)
        };

    public void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus) =>
        rootMenus.OperationalAdmin.Children.Add(new MenuDTO(Routes.MenuDesigner, "menu"));
}
