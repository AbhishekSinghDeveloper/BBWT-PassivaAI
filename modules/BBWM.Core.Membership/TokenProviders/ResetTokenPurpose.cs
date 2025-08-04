using BBWM.Core.Membership.Model;

using Microsoft.AspNetCore.Identity;

namespace BBWM.Core.Membership.TokenProviders;

public static class ResetTokenPurpose
{
    public const string ResetPassword = UserManager<User>.ResetPasswordTokenPurpose;

    public const string UserInvite = "UserInvite";
}
