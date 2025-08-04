using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;

namespace BBWT.InitialData;

/// This routes related to the project-specific pages routing.
/// Add project features pages routes into the route roles list of this class.
/// (e.g. new PageInfoDTO("/app/my-feature", "My Feature",  Roles.MyFeatureManager))
/// As this class implements IRouteRolesModule, it's automatically collected by dependency injection.
public class RouteRoles : IRouteRolesModule
{
    public List<PageInfoDTO> GetRouteRoles() =>
        new List<PageInfoDTO>
        {

        };
}
