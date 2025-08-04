using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;

namespace BBWM.Core.Membership.Interfaces;

public interface IRoleService :
    IEntityQuery<Role>,
    IEntityCreate<RoleDTO>,
    IEntityUpdate<RoleDTO>,
    IEntityDelete<string>,
    IEntityPage<RoleDTO>

{
    IEnumerable<RoleDTO> GetHardcodedRoles();
    IEnumerable<RoleDTO> GetProjectRoles();
    Task CleanupRoles(CancellationToken cancellationToken = default);
}
