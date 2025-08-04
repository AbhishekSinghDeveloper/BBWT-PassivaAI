using BBWM.Core.Membership.DTO;

namespace BBWM.Core.Membership.Interfaces;

public interface IPermissionService
{
    Task<ICollection<PermissionDTO>> GetAll(CancellationToken cancellationToken = default);
    Task CleanupPermissions(CancellationToken cancellationToken = default);
}
