using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.TokenProviders;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

using System.Security.Cryptography;
using System.Text;

namespace BBWM.Core.Membership.Extensions;

public static class UserManagerExtensions
{
    private static readonly RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();

    public static async Task<string> GenerateUserInviteTokenAsync<TUser>(
        this UserManager<TUser> userManager, TUser user)
        where TUser : class
    {
        var token = await userManager.GenerateUserTokenAsync(
            user,
            PasswordResetTokenProvider.ProviderName,
            ResetTokenPurpose.UserInvite);

        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    public static async Task<IdentityResult> ResetInvitePasswordAsync<TUser>(
        this UserManager<TUser> userManager, TUser user, string token, string newPassword)
        where TUser : class
    {
        if (user is null) throw new ArgumentNullException(nameof(user));

        if (!await userManager.VerifyUserTokenAsync(
            user, PasswordResetTokenProvider.ProviderName, ResetTokenPurpose.UserInvite, token))
            return userManager.InvalidToken();

        return await userManager.AddPasswordAsync(user, newPassword);
    }

    public static async Task UpdateAuthSecurityStampAsync<TUser>(this UserManager<TUser> userManager, TUser user)
        where TUser : User
    {
        user.AuthSecurityStamp = NewAuthSecurityStamp();
        await userManager.UpdateAsync(user);
    }

    internal static string NewAuthSecurityStamp()
    {
        var bytes = new byte[20];
        _randomNumberGenerator.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static IdentityResult InvalidToken<TUser>(this UserManager<TUser> userManager)
        where TUser : class
        => IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
}
