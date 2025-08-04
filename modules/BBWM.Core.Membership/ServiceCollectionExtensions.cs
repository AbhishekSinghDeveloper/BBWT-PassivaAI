using BBWM.Core.Membership.Authorization;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.TokenProviders;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core.Membership;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlServerSignInManager<TContext>(
        this IServiceCollection services, Action<IdentityOptions> identityOptions) where TContext : DbContext
            => AddSignInManager<TContext>(services, identityOptions);

    public static IServiceCollection AddMySqlSignInManager<TContext>(
        this IServiceCollection services, Action<IdentityOptions> identityOptions)
        where TContext : DbContext
            => AddSignInManager<TContext>(services, identityOptions);

    public static IServiceCollection AddSignInManager<TContext>(
        IServiceCollection services, Action<IdentityOptions> identityOptions)
        where TContext : DbContext
    {
        services
            .AddUserIdentity(identityOptions)
            .AddEntityFrameworkStores<TContext>()
            .AddDefaultTokenProviders()
            .AddUserStore<UserStore<User, Role, TContext, string, IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>>()
            .AddRoleStore<RoleStore<Role, TContext, string, UserRole, IdentityRoleClaim<string>>>()
            .AddSignInManager<AuditableSignInManager>()
            .OverrideSecurityStampValidator();

        return services;
    }

    private static IdentityBuilder AddUserIdentity(this IServiceCollection services, Action<IdentityOptions> config)
        => services
        .AddIdentity<User, Role>(options =>
        {
            config?.Invoke(options);

            if (options.Tokens.PasswordResetTokenProvider == TokenOptions.DefaultProvider)
                options.Tokens.PasswordResetTokenProvider = PasswordResetTokenProvider.ProviderName;

            if (options.Tokens.EmailConfirmationTokenProvider == TokenOptions.DefaultProvider)
                options.Tokens.EmailConfirmationTokenProvider = EmailConfirmationTokenProvider.ProviderName;
        })
        .AddUserManager<BbwtUserManager<User>>()
        .AddTokenProvider<EmailConfirmationTokenProvider>(EmailConfirmationTokenProvider.ProviderName)
        .AddTokenProvider<PasswordResetTokenProvider>(PasswordResetTokenProvider.ProviderName);

    /// <summary>
    /// Replaces default <see cref="SecurityStampValidator{TUser}"/> and <see cref="TwoFactorSecurityStampValidator{TUser}"/>
    /// Security Stamp validators by custom validators from the DI container.
    /// </summary>
    /// <remarks>
    /// See <see cref="AuthSecurityStampValidator{TUser}"/> and <see cref="AuthTwoFactorSecurityStampValidator{TUser}"/>
    /// validators.
    /// </remarks>
    /// <param name="builder">The Identity builder</param>
    /// <returns>The Identity builder</returns>
    private static IdentityBuilder OverrideSecurityStampValidator(this IdentityBuilder builder)
    {
        builder.Services.AddScoped(
            typeof(ISecurityStampValidator),
            typeof(AuthSecurityStampValidator<>).MakeGenericType(builder.UserType));
        builder.Services.AddScoped(
            typeof(ITwoFactorSecurityStampValidator),
            typeof(AuthTwoFactorSecurityStampValidator<>).MakeGenericType(builder.UserType));

        return builder;
    }
}
