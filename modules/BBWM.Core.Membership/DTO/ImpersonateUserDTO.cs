namespace BBWM.Core.Membership.DTO;

public class ImpersonateUserDTO
{
    public string ImpersonatedUserId { get; set; }
    public string ImpersonatedUserName { get; set; }
    public string ImpersonatedUserEmail { get; set; }
    public string OriginalUserName { get; set; }
    public bool IsImpersonating { get; set; }
}
