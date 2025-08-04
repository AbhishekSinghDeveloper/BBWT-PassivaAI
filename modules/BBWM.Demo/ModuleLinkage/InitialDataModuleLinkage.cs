using BBWM.Core.Data;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Utils;
using BBWM.Core.ModuleLinker;
using BBWM.Demo.Guidelines;
using BBWM.Demo.Security;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using DemoFile = BBWM.Demo.Guidelines.File;

namespace BBWM.Demo.ModuleLinkage;

public class InitialDataModuleLinkage : IInitialDataModuleLinkage
{
    private IUserService _userService;
    private IUserInitializeService _userInitializeService;
    private RoleManager<Role> _roleManager;
    private UserManager<User> _userManager;

    // This role name should NOT be used as a hardcoded role being set for API/pages routes. The demo roles should present in
    // the Roles tables only, as if were added in run-time on the website.
    private const string AccessControlDemoManagerRole = "Access Control Demo – Manager";

    public async Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        if (includingOnceSeededData)
        {
            _userService = serviceScope.ServiceProvider.GetService<IUserService>();
            _userInitializeService = serviceScope.ServiceProvider.GetService<IUserInitializeService>();
            _roleManager = serviceScope.ServiceProvider.GetService<RoleManager<Role>>();
            _userManager = serviceScope.ServiceProvider.GetService<UserManager<User>>();

            // Init data for the main database
            var context = serviceScope.ServiceProvider.GetService<IDbContext>();
            await CreateInitialPermissions(context);
            await CreateInitialRoles(context);
            await CreateInitialGroups(context);
            await CreateInitialUsers(context);

            // Init data for the demo database
            var demoDbContext = serviceScope.ServiceProvider.GetService<IDemoDataContext>();
            await InitDemoDbData(demoDbContext);
        }
    }

    private static async Task CreateInitialPermissions(IDbContext context)
    {
        var allPermissionNames = PermissionsExtractor.GetPermissionNamesOfClass(typeof(Permissions));

        foreach (var permissionName in allPermissionNames)
        {
            if (await context.Set<Permission>().AnyAsync(x => x.Name == permissionName)) continue;

            var permission = new Permission(permissionName);
            await context.Set<Permission>().AddAsync(permission);
            await context.SaveChangesAsync();
        }
    }

    private async Task CreateInitialRoles(IDbContext context)
    {
        #region Initial demo roles and their permissions
        var rolesDto = new List<RoleDTO> {
                new RoleDTO {
                    Name = AccessControlDemoManagerRole,
                    Permissions = new List<PermissionDTO> { new PermissionDTO { Name = Permissions.AccessControlDemoViewOrders } }
                }
            };
        #endregion

        foreach (var roleDto in rolesDto)
        {
            var role = await _roleManager.FindByNameAsync(roleDto.Name);
            if (role is null)
            {
                // For a project a role's ID is a randomly generated GUID, but for the Demo module it's important that the ID is
                // hardcoded here. In a project we should NOT seed initial roles using the permissions based model because there we manage
                // roles in runtime, but for the Demo module we artificially seed initial roles for demonstration purposes. It conflicts
                // with seeding that comes from the roles.json file (when a roles set comes from git's repo). In order to avoid
                // the conflict the demo roles are seeded with constant IDs so they match the roles.json's ones.
                role = new Role(roleDto.Name) { Id = roleDto.Name.ToUpperInvariant().Replace(' ', '_') };
                await _roleManager.CreateAsync(role);

                if (roleDto.Permissions.Any())
                {
                    foreach (var permissionDto in roleDto.Permissions)
                    {
                        var permission = await context.Set<Permission>().SingleOrDefaultAsync(o => o.Name == permissionDto.Name);
                        await context.Set<RolePermission>().AddAsync(
                            new RolePermission
                            {
                                RoleId = role.Id,
                                PermissionId = permission.Id
                            });
                    }

                    await context.SaveChangesAsync();
                }
            }
        }
    }

    private static async Task CreateInitialGroups(IDbContext context)
    {
        var allGroupNames = GroupsExtractor.GetGroupNamesOfClass(typeof(Groups));

        foreach (var groupName in allGroupNames)
        {
            var group = await context.Set<Group>().FirstOrDefaultAsync(x => x.Name == groupName);

            if (group is null)
            {
                group = new Group { Name = groupName };
                await context.Set<Group>().AddAsync(group);
                await context.SaveChangesAsync();
            }

            switch (groupName)
            {
                case Groups.GroupA: Groups.IdGroupA = group.Id; break;
                case Groups.GroupB: Groups.IdGroupB = group.Id; break;
                default:
                    break;
            }
        }
    }

    private async Task CreateInitialUsers(IDbContext context)
    {
        await _userInitializeService.CreateInitialUser(InitialUsers.DemoAdmin,
            roles: new[] { Core.Roles.SuperAdminRole, Core.Roles.SystemAdminRole },
            permissions: new[] { Permissions.AccessControlDemoViewNote1, Permissions.AccessControlDemoViewNote2 });
        await _userInitializeService.CreateInitialUser(InitialUsers.DemoUser);
        await _userInitializeService.CreateInitialUser(InitialUsers.Manager, AccessControlDemoManagerRole);
        await _userInitializeService.CreateInitialUser(
            InitialUsers.ManagerInGroupA,
            roles: new[] { AccessControlDemoManagerRole },
            permissions: default,
            groups: new[] { Groups.GroupA });
        await _userInitializeService.CreateInitialUser(
            InitialUsers.ManagerInGroupB,
            roles: new[] { AccessControlDemoManagerRole },
            permissions: default,
            groups: new[] { Groups.GroupB });
        await _userInitializeService.CreateInitialUser(InitialUsers.ManagerInGroupAB,
            roles: new[] { AccessControlDemoManagerRole }, permissions: null, groups: new[] { Groups.GroupA, Groups.GroupB });
    }

    private async Task InitDemoDbData(IDbContext context)
    {
        // seed Files
        if (!context.Set<DemoFile>().Any())
        {
            context.Set<DemoFile>().AddRange(GenerateFilesTree());
            await context.SaveChangesAsync();
        }
    }

    private static List<DemoFile> GenerateFilesTree()
    {
        var files = new List<DemoFile> {
                new () { Label = "Documents", Data = "Documents", ParentId = null, Type = FileType.Folder },
                new () { Label = "Photos", Data = "Photos", ParentId = null, Type = FileType.Folder },
                new () { Label = "Belarus.jpg", Data = "Belarus", ParentId = null, Type = FileType.File },
                new () { Label = "Venezuela.png", Data = "Venezuela", ParentId = null, Type = FileType.File }
              };
        return files;
    }
}
