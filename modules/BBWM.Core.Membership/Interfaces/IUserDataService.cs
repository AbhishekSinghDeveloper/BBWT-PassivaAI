using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;

namespace BBWM.Core.Membership.Interfaces;

public interface IUserDataService : ICrudHandler<User, UserDTO, UserDTO, string>
{
    Task<bool> Exists(string id, CancellationToken cancellationToken = default);
    Task<UserDTO> GetByEmail(string email, CancellationToken cancellationToken = default);
    Task<ICollection<UserDTO>> ReplaceUsersRoles(UsersRolesReplacementDTO dto, CancellationToken cancellationToken = default);
    Task<ICollection<UserDTO>> ReplaceUsersGroups(UsersGroupsReplacementDTO dto, CancellationToken cancellationToken = default);
    Dictionary<string, object> GetAllAccountStatuses(CancellationToken cancellationToken = default);
    Task<IEnumerable<RoleDTO>> GetAllRoles(CancellationToken cancellationToken = default);
    Task<IEnumerable<GroupDTO>> GetAllGroups(CancellationToken cancellationToken = default);
    Task<UserSignatureDTO> GetUserSignature(string userId, CancellationToken cancellationToken = default);
    Task<bool> SetUserSignature(string userId, string signature, CancellationToken cancellationToken = default);

}
