using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;
using BBWM.Menu;
using BBWM.Menu.DTO;

using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Messages.Templates.Api;

public class ModuleLinkage : IRouteRolesModuleLinkage, IMenuModuleLinkage
{
    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope) =>
        new List<PageInfoDTO>
        {
                new PageInfoDTO(Routes.EmailTemplates, Core.Roles.SystemAdminRole),
                new PageInfoDTO(Routes.EmailTemplatesDetails, Core.Roles.SystemAdminRole)
        };

    public void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus) =>
        rootMenus.OperationalAdmin.Children.Add(new MenuDTO(Routes.EmailTemplates, "settings"));
}
