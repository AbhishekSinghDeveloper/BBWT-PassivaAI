using BBWM.Core.Membership.Extensions;
using BBWM.Core.Membership.Model;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BBWM.Core.Membership;

/// <summary>
/// Custom user manager designed to setup an Authentication Security Stamp on the
/// <typeparamref name="TUser"/> at creation time.
/// </summary>
/// <remarks>
/// See <see cref="AuthSecurityStampValidator{TUser}"/>
/// </remarks>
/// <typeparam name="TUser">User type</typeparam>
public class BbwtUserManager<TUser> : UserManager<TUser> where TUser : User
{
    public BbwtUserManager(
        IUserStore<TUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<TUser> passwordHasher,
        IEnumerable<IUserValidator<TUser>> userValidators,
        IEnumerable<IPasswordValidator<TUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<TUser>> logger)
        : base(
              store,
              optionsAccessor,
              passwordHasher,
              userValidators,
              passwordValidators,
              keyNormalizer,
              errors,
              services,
              logger)
    { }

    // Note: We can't implement extension methods in this class as we would need to update all
    //       UserManger<TUser> references with this one

    public override Task<IdentityResult> CreateAsync(TUser user)
    {
        SetAuthSecurityStamp(user);
        return base.CreateAsync(user);
    }

    public override Task<IdentityResult> CreateAsync(TUser user, string password)
    {
        SetAuthSecurityStamp(user);
        return base.CreateAsync(user, password);
    }

    private void SetAuthSecurityStamp(TUser user)
    {
        ThrowIfDisposed();
        user.AuthSecurityStamp = UserManagerExtensions.NewAuthSecurityStamp();
    }
}
