using BBWM.Core.Membership.DTO;

namespace BBWM.Core.Membership.Interfaces;

public interface IUserInitializeService
{
    Task CreateInitialUser(UserDTO initialUser, string role = null);
    Task CreateInitialUser(UserDTO initialUser, string[] roles);
    Task CreateInitialUser(UserDTO initialUser, string[] roles, string[] permissions, string[] groups = null);
}
