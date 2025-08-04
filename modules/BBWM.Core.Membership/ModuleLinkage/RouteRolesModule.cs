using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Enums;

namespace BBWM.Core.Membership.ModuleLinkage;

public class RouteRolesModule : IRouteRolesModule
{
    private readonly IApiAccessModelGetter _apiAccessModelGetter;

    public RouteRolesModule(IApiAccessModelGetter apiAccessModelGetter)
    {
        _apiAccessModelGetter = apiAccessModelGetter;
    }

    public List<PageInfoDTO> GetRouteRoles()
    {
        return new List<PageInfoDTO>
        {
            new PageInfoDTO(Routes.LoginAudit, Roles.SystemAdminRole),
            new PageInfoDTO(Routes.Users, Roles.SystemAdminRole),
            new PageInfoDTO(Routes.UsersDetails, Roles.SystemAdminRole),
            new PageInfoDTO(Routes.Roles,
                _apiAccessModelGetter.GetApiAccessModel() == ApiAccessModel.PermissionBased ? Roles.SuperAdminRole : null),
            new PageInfoDTO(Routes.Organizations, Roles.SystemAdminRole),
            new PageInfoDTO(Routes.OrganizationDetails, Roles.SystemAdminRole),
            new PageInfoDTO(Routes.AllowedIpDetails) {
                Roles = new List<string> {
                    Roles.SystemAdminRole, Roles.SuperAdminRole}
            },
            new PageInfoDTO(Routes.RoutesAccess, Roles.SuperAdminRole)
        };
    }
}
