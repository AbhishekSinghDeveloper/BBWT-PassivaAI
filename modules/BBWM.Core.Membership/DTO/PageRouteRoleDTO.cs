namespace BBWM.Core.Membership.DTO;

public class PageRouteRoleDTO
{
    public IEnumerable<PageInfoDTO> PageRoles { get; set; }
    public IEnumerable<ApiEndPointInfoDTO> RouteRoles { get; set; }
}
