using BBWM.Core;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;

namespace BBWT.InitialData;

/// This routes related to core modules and maybe sorted later by BBWT3 team. Normally They should not
/// be modified unless it's required to remove / change roles&permissions.
/// As this class implements IRouteRolesModule, it's automatically collected by dependency injection.
public class RouteRolesCore : IRouteRolesModule
{
    public List<PageInfoDTO> GetRouteRoles() =>
        new List<PageInfoDTO>
        {
                new PageInfoDTO(CoreRoutes.Home, AggregatedRoles.Anyone),
                new PageInfoDTO(BBWM.SystemSettings.Routes.SystemConfiguration) {
                    Roles = new List<string> { BBWM.Core.Roles.SystemAdminRole, BBWM.Core.Roles.SuperAdminRole }
                },
                new PageInfoDTO("/app/report-problem", "Report a Problem", AggregatedRoles.Authenticated),
                new PageInfoDTO("/app/profile", "Profile", AggregatedRoles.Authenticated),
                new PageInfoDTO("/app/profile/authentication", "Authentication", AggregatedRoles.Authenticated)
        };
}
