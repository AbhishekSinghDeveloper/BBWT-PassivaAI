using BBWM.Core;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;

using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Demo.Security;

public class RouteRolesModuleLinkage : IRouteRolesModuleLinkage
{
    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope) =>
        new List<PageInfoDTO>
        {
                new PageInfoDTO(Routes.SecurityReadMeFirst, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.SecurityAnyAuthenticated, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.SecurityGroups, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.SecurityGroupA)
                    .ForPermission(Permissions.AccessControlDemoViewOrders)
                    .ForGroup(Groups.GroupA),
                new PageInfoDTO(Routes.SecurityGroupB)
                    .ForPermission(Permissions.AccessControlDemoViewOrders)
                    .ForGroup(Groups.GroupB),
                new PageInfoDTO(Routes.SecurityNote1).ForPermission(Permissions.AccessControlDemoViewNote1),
                new PageInfoDTO(Routes.SecurityNote2).ForPermission(Permissions.AccessControlDemoViewNote2),
        };
}
