using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;
using BBWM.Menu;
using BBWM.Menu.DTO;
using Microsoft.Extensions.DependencyInjection;

namespace BBF.Reporting.Web;

public class ReportingWebModuleLinkage :
    IRouteRolesModuleLinkage,
    IMenuModuleLinkage
{
    public void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus)
    {
        menu.InsertRange(0, new List<MenuDTO>
        {
            new(Dashboard.Api.Routes.Dashboards, "web"),
            new(Routes.Queries, "code"),
            new(Routes.Widgets, "widgets"),
        });
    }

    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope) =>
        new()
        {
            new PageInfoDTO(Routes.Queries, BBWM.Core.AggregatedRoles.Authenticated),
            new PageInfoDTO(Routes.QueryCreate, BBWM.Core.AggregatedRoles.Authenticated),
            new PageInfoDTO(Routes.QueryEdit, BBWM.Core.AggregatedRoles.Authenticated),
            new PageInfoDTO(Routes.Widgets, BBWM.Core.AggregatedRoles.Authenticated),
            new PageInfoDTO(Routes.WidgetCreate, BBWM.Core.AggregatedRoles.Authenticated),
            new PageInfoDTO(Routes.WidgetEdit, BBWM.Core.AggregatedRoles.Authenticated),
        };
}