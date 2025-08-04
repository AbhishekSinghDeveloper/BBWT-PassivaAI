using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.SystemSettings;
using BBWM.Core.Membership.Utils;
using BBWM.Core.ModuleLinker;
using BBWM.SystemSettings;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BBWM.Core.Membership.ModuleLinkage;

public class DataModuleLinkage :
    IInitialDataModuleLinkage,
    IDbModelCreateModuleLinkage
{
    private IServiceScope _serviceScope;
    private RoleManager<Role> _roleManager;
    private ISettingsService _settingsService;

    public async Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        _serviceScope = serviceScope;

        _roleManager = _serviceScope.ServiceProvider.GetService<RoleManager<Role>>();
        _settingsService = _serviceScope.ServiceProvider.GetService<ISettingsService>();

        await CreateInitialRoles();
        await CreateInitialSystemSettings();
    }

    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.Entity<LoginAudit>();
        builder.Entity<Address>();
        builder.Entity<Organization>();
        builder.Entity<UserPasswordFailedHistory>();
        builder.Entity<PasswordHistory>();
        builder.Entity<Device>();
        builder.Entity<AuthenticationRequest>();
        builder.Entity<Branding>();
        builder.Entity<AllowedIp>();
        builder.Entity<LockedOutIp>();
        builder.Entity<AllowedIpUser>();
        builder.Entity<AllowedIpRole>();
        builder.Entity<UserGroup>();
        builder.Entity<UserOrganization>();
        builder.Entity<ActivationToken>();
        builder.Entity<Permission>();
        builder.Entity<RolePermission>();
        builder.Entity<UserPermission>();
        builder.Entity<Model.Metadata>();

        // Registering models configurations of the module for the main project's DB context
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private async Task CreateInitialRoles()
    {
        var roleNames = RolesExtractor.GetRolesNamesOfClass(typeof(Roles));

        foreach (var roleName in roleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new Role(roleName) { Id = Guid.NewGuid().ToString() };
                await _roleManager.CreateAsync(role);
            }
        }
    }

    private async Task CreateInitialSystemSettings()
    {
        var appSettings = new[]
        {
            new SettingsDTO { Value = _settingsService.GetSettingsSection<UserPasswordSettings>() ?? new UserPasswordSettings() },
            new SettingsDTO { Value = _settingsService.GetSettingsSection<FailedAttemptsPasswordSettings>() ?? new FailedAttemptsPasswordSettings() },
            new SettingsDTO { Value = _settingsService.GetSettingsSection<UserSessionSettings>() ?? new UserSessionSettings() },
            new SettingsDTO { Value = _settingsService.GetSettingsSection<RegistrationSettings>() ?? new RegistrationSettings() },
            new SettingsDTO { Value = _settingsService.GetSettingsSection<TwoFactorSettings>() ?? new TwoFactorSettings() }
        };

        await _settingsService.Save(appSettings);
    }
}