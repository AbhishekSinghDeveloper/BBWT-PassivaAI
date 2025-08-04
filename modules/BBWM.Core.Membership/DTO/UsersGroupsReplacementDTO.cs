namespace BBWM.Core.Membership.DTO;

public class UsersGroupsReplacementDTO
{
    public ICollection<string> UsersIds { get; set; }
    public ICollection<string> GroupsIdsToAdd { get; set; }
    public ICollection<string> GroupsIdsToRemove { get; set; }
}
