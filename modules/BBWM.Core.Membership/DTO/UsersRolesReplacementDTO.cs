namespace BBWM.Core.Membership.DTO;

public class UsersRolesReplacementDTO
{
    public ICollection<string> UsersIds { get; set; }
    public ICollection<string> RolesIdsToAdd { get; set; }
    public ICollection<string> RolesIdsToRemove { get; set; }
}
