using BBWM.Core;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;

using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Demo.S3FileManager;

public class RouteRolesModuleLinkage : IRouteRolesModuleLinkage
{
    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope) =>
        new List<PageInfoDTO>
        {
                new PageInfoDTO(Routes.S3FileManager, AggregatedRoles.Authenticated)
        };
}
