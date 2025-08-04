using BBWM.Core;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;

using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Demo.SimulateError;

public class RouteRolesModuleLinkage : IRouteRolesModuleLinkage
{
    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope) =>
        new List<PageInfoDTO>
        {
                new PageInfoDTO(Routes.SimulateError, AggregatedRoles.Authenticated)
        };
}
