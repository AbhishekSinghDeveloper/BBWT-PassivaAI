using BBWM.Core;
using BBWM.Core.AppEnvironment;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership;
using BBWM.Core.Membership.Enums;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;

namespace BBWT.InitialData;

public partial class DatabaseInitializerService : IDatabaseInitializerService
{
    private readonly IDbContext _context;
    private readonly ILogger<DatabaseInitializerService> _logger;
    private readonly RoleManager<Role> _roleManager;
    private readonly IApiAccessModelGetter _apiAccessModelGetter;
    private readonly IRoleGitDataService _roleGitDataService;
    private readonly IPermissionService _permissionService;
    private readonly IUserInitializeService _userInitializeService;
    private readonly IUserService _userService;
    private readonly IAppEnvironmentService _appEnvironmentService;

    public DatabaseInitializerService(
        IDbContext context,
        ILogger<DatabaseInitializerService> logger,
        RoleManager<Role> roleManager,
        IApiAccessModelGetter apiAccessModelGetter,
        IRoleGitDataService roleGitDataService,
        IPermissionService permissionService,
        IUserInitializeService userInitializeService,
        IUserService userService,
        IAppEnvironmentService appEnvironmentService)
    {
        _context = context;
        _logger = logger;
        _roleManager = roleManager;
        _apiAccessModelGetter = apiAccessModelGetter;
        _roleGitDataService = roleGitDataService;
        _permissionService = permissionService;
        _userInitializeService = userInitializeService;
        _userService = userService;
        _appEnvironmentService = appEnvironmentService;
    }

    public void EnsureInitialData(bool includingOnceSeededData)
    {
        if (!AllMigrationsApplied())
        {
            _logger.LogError("DatabaseInitializerService: not all migrations applied");
            return;
        }

        // Data initialization is wrapped into try/catch to avoid any error to failure the portal starting
        try
        {
            CreateInitialRolesAndPermissions();

            // These DB records should be created only once
            if (includingOnceSeededData)
            {
                CreateGroups();
                CreateOrganizations();
                CreateUsers();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ensure DB initial data failure");
        }

        SyncRolesAndPermissions();
    }

    private void CreateInitialRolesAndPermissions()
    {
        switch (_apiAccessModelGetter.GetApiAccessModel())
        {
            case ApiAccessModel.RoleBased:
                CreateInitialRoles();
                break;

            case ApiAccessModel.PermissionBased:
                CreateInitialPermissions();
                break;

            default: break;
        }
    }

    /// <summary>
    /// Sync roles & permissions from git's roles.json
    /// This action is(should be) done after all initial roles & permissions of modules & project are seeded
    /// </summary>
    private void SyncRolesAndPermissions()
    {
        try
        {
            switch (_apiAccessModelGetter.GetApiAccessModel())
            {
                case ApiAccessModel.PermissionBased:
                    _permissionService.CleanupPermissions().Wait();
                    _roleGitDataService.UpdateRolesFromJson().Wait();
                    break;

                case ApiAccessModel.RoleBased:
                    // NOTE for BBWT3 team! If we cleanup hardcoded roles using the roles based model then the soft
                    // coded roles of the demo module are removed, but they shouldn't. This needs to be resolved
                    // first or we don't clean up roles at all. (_roleService.CleanupRoles().Wait();)
                    break;

                default: break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync roles & permissions from git's roles.json");

            // This point throws a special critical exception so the startup stops the app if it's being run from
            // the migration job (with -migrate parameter)
            throw new DataInitCriticalException("Sync roles & permissions from git's roles.json", ex);
        }
    }

    private bool AllMigrationsApplied()
    {
        var applied = ((DbContext)_context).GetService<IHistoryRepository>().GetAppliedMigrations().Select(m => m.MigrationId);
        var total = ((DbContext)_context).GetService<IMigrationsAssembly>().Migrations.Select(m => m.Key);
        return !total.Except(applied).Any();
    }

    private void CreateInitialPermissions()
    {
        var allPermissionsNames = PermissionsExtractor.GetPermissionNamesOfClass(typeof(Services.Permissions));

        foreach (var permissionName in allPermissionsNames)
        {
            if (_context.Set<Permission>().Any(x => x.Name == permissionName)) continue;

            var permission = new Permission(permissionName);
            _context.Set<Permission>().Add(permission);
            _context.SaveChanges();
        }
    }

    private void CreateInitialRoles()
    {
        var roleNames = RolesExtractor.GetRolesNamesOfClass(typeof(Services.Roles));

        foreach (var roleName in roleNames)
        {
            var role = _roleManager.FindByNameAsync(roleName).Result;
            if (role is null)
            {
                role = new Role(roleName) { Id = Guid.NewGuid().ToString() };
                _roleManager.CreateAsync(role).Wait();
            }
        }
    }

    /// <summary>
    /// Seeds initial groups
    /// </summary>
    private void CreateGroups()
    {
        if (_appEnvironmentService.IsLiveTypeEnvironment())
        {
            // To be populated by the customer project
        }
        else
        {
            var allGroupsNames = GroupsExtractor.GetGroupNamesOfClass(typeof(Services.Groups));

            foreach (var groupName in allGroupsNames)
            {
                if (_context.Set<Group>().Any(x => x.Name == groupName)) continue;

                var group = new Group { Name = groupName };
                _context.Set<Group>().Add(group);
                _context.SaveChanges();
            }
        }
    }

    /// <summary>
    /// Seeds initial organizations
    /// </summary>
    private void CreateOrganizations()
    {
        if (_appEnvironmentService.IsLiveTypeEnvironment())
        {
            // To be populated by the customer project
        }
        else
        {
            // Creating demo organizations that maybe useful on pre-production environment phases (local development / test site etc)
            var demoOrganizationName = "SuperOrg";
            if (!_context.Set<Organization>().Any(x => x.Name == demoOrganizationName))
            {
                _context.Set<Organization>().AddRange(
                    new List<Organization> {
                        new() { Name = demoOrganizationName }
                    });
                _context.SaveChanges();
            }
        }
    }

    /// <summary>
    /// If you need to seed customer project specific users, rewrite or extend this method.
    /// Creating initial user who logs in the system the very first time
    /// For Production/UAT envirnoments it's supposed to be the only user who deploy/initiate
    /// the application for the production environment)
    /// For Development/Test environments it additionally may create demo users useful on pre-production environment phases
    /// </summary>
    private void CreateUsers()
    {
        if (_appEnvironmentService.IsLiveTypeEnvironment())
        {
            // For live environment we create a single user with both core roles so he could completely configure the system quickly
            // from the start.
            _userInitializeService.CreateInitialUser(
                InitialUsers.SuperAdmin,
                new string[] { Roles.SuperAdminRole, Roles.SystemAdminRole })
            .Wait();
        }
        else
        {
            _userInitializeService.CreateInitialUser(InitialUsers.SuperAdmin, Roles.SuperAdminRole).Wait();
            _userInitializeService.CreateInitialUser(InitialUsers.SystemAdmin, Roles.SystemAdminRole).Wait();
        }
    }
}