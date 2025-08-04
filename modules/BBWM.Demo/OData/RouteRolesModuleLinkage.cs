using BBWM.Core;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;

using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Demo.OData;

public class RouteRolesModuleLinkage : IRouteRolesModuleLinkage
{
    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope) =>
        new List<PageInfoDTO>
        {
            /* OData is temporarily disabled because of unresolved issues faced after the NET 5 update.
             * The detailed investigation results are written in the https://pts.bbconsult.co.uk/issueEditor?id=254602 issue.*/
            new PageInfoDTO(Routes.OData, AggregatedRoles.Authenticated)
        };
}
