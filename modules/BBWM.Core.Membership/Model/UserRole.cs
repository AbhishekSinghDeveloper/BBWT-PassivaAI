using Microsoft.AspNetCore.Identity;

namespace BBWM.Core.Membership.Model;

public class UserRole : IdentityUserRole<string>
{
    public User User { get; set; }

    public Role Role { get; set; }
}
