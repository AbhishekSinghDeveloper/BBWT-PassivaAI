using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.SystemSettings;
using BBWM.Core.Services;
using BBWM.SystemSettings;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Security.Claims;

using BbwtClaimTypes = BBWM.Core.Membership.Model.ClaimTypes;

namespace BBWM.Core.Membership.Authorization;

public class BBWT3UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, Role>
{
    private readonly ISettingsService _settingsService;

    public BBWT3UserClaimsPrincipalFactory(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        ISettingsService settingsService)
        : base(userManager, roleManager, optionsAccessor)
    {
        _settingsService = settingsService;
    }


    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var id = await base.GenerateClaimsAsync(user);

        // Organization claim
        if (user.UserOrganizations.Any())
        {
            // TODO: consider multiple organizations.
            id.AddClaim(new Claim(MultiTenancyService.TenantField, user.UserOrganizations.First().OrganizationId.ToString()));
        }
        else if (user.OrganizationId is not null)
        {
            id.AddClaim(new Claim(MultiTenancyService.TenantField, user.OrganizationId.ToString()));
        }

        // Permissions claims
        user = await UserManager.Users
            .Include(x => x.UserPermissions)
            .ThenInclude(x => x.Permission)
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .ThenInclude(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .SingleOrDefaultAsync(x => x.Id == user.Id);
        var userPermissionNames = user.UserPermissions.Select(x => x.Permission.Name)
            .Union(user.UserRoles.Select(x => x.Role).SelectMany(x => x.RolePermissions).Select(x => x.Permission.Name))
            .Distinct();
        id.AddClaims(userPermissionNames.Select(x => new Claim(x, "true")));

        //2FA claim
        var isUserRequiredTwoFactorSetup = IsUserRequiredSetupTwoFactor(user);
        id.AddClaim(new Claim(BbwtClaimTypes.Authentication.UserRequiredSetupTwoFactor, isUserRequiredTwoFactorSetup.ToString()));

        // Auth security stamp claim
        id.AddClaim(
            new Claim(BbwtClaimTypes.Authentication.AuthSecurityStamp, user.AuthSecurityStamp ?? string.Empty));

        return id;
    }

    #region private helpers

    private bool IsUserRequiredSetupTwoFactor(User user)
    {
        var globalTwoFactorMode = _settingsService.GetSettingsSection<TwoFactorSettings>().MandatoryMode;
        var userRoles = user.UserRoles;

        var isSetupTwoFactorCheckRequired = false;

        // Users with SuperAdmin role is excluded from being demanded to set up 2FA in order not to block the whole site's UI.
        // At least SuperAdmin user should access site pages.
        if (userRoles.All(x => x.Role.Name != Roles.SuperAdminRole))
        {
            isSetupTwoFactorCheckRequired = globalTwoFactorMode switch
            {
                TwoFactorMandatoryMode.Mandatory => true,
                TwoFactorMandatoryMode.MandatoryForSpecificRoles => userRoles.Any() && userRoles.Any(x => x.Role.AuthenticatorRequired),
                TwoFactorMandatoryMode.Optional => false,
                _ => false
            };
        }

        return isSetupTwoFactorCheckRequired && !(user.U2fEnabled || user.TwoFactorEnabled);
    }

    #endregion
}
