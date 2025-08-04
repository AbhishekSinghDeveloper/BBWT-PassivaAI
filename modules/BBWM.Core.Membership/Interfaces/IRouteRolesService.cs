using BBWM.Core.Membership.DTO;

namespace BBWM.Core.Membership.Interfaces;

public interface IRouteRolesService
{
    IEnumerable<ApiEndPointInfoDTO> GetApiRoutesRoles();
    IEnumerable<PageInfoDTO> GetPagesRoutes();
    Task<string[]> GetPageRoutesForUser(string userId, CancellationToken cancellationToken = default);
}
