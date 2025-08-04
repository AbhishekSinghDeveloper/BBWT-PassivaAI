using BBWM.Core;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;

namespace BBWM.Scheduler;
public class RouteRolesModule : IRouteRolesModule
{
    public List<PageInfoDTO> GetRouteRoles()
    {
        return new List<PageInfoDTO>()
        {
            new PageInfoDTO(Routes.SchedulerDashboard, Roles.SuperAdminRole),
            new PageInfoDTO(Routes.SchedulerJobs, Roles.SuperAdminRole),
            new PageInfoDTO(Routes.SchedulerRecurringJobs, Roles.SuperAdminRole),
            new PageInfoDTO(Routes.SchedulerRetries, Roles.SuperAdminRole),
            new PageInfoDTO(Routes.SchedulerServers, Roles.SuperAdminRole),

        };
    }
}
