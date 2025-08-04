using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;


namespace BBWM.AggregatedLogs;

public class RouteRolesModule : IRouteRolesModule
{
    public List<PageInfoDTO> GetRouteRoles()
        => new()
        {
            new PageInfoDTO(Routes.Logs, new List<string> { Core.Roles.SuperAdminRole, Core.Roles.SystemAdminRole })
        };
}