using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Enums;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using ClaimTypes = BBWM.Core.Membership.Model.ClaimTypes;

namespace BBWM.Core.Membership.Services;

public class UserInitializeService : IUserInitializeService
{
    private readonly IDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ISecurityService _securityService;
    private readonly IMapper _mapper;

    public UserInitializeService(
        UserManager<User> userManager,
        ISecurityService securityService,
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _securityService = securityService;
        _mapper = mapper;
    }

    public Task CreateInitialUser(UserDTO initialUser, string role = null)
        => CreateInitialUser(initialUser, role is null ? initialUser.Roles.Select(x => x.Name).ToArray() : new[] { role }, null, null);

    public Task CreateInitialUser(UserDTO initialUser, string[] roles)
        => CreateInitialUser(initialUser, roles, null, null);

    public async Task CreateInitialUser(UserDTO initialUser, string[] roles, string[] permissions, string[] groups = null)
    {
        var isCreated = await SeedUser(initialUser);

        // If user is just created then we add his roles/permissions, otherwise we don't change existing values
        if (isCreated)
        {
            var user = await _userManager.FindByNameAsync(initialUser.Email);

            if (roles is not null)
            {
                foreach (var role in roles)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            if (permissions is not null)
            {
                foreach (var permissionName in permissions)
                {
                    var permission = await _context.Set<Permission>().FirstOrDefaultAsync(o => o.Name == permissionName);

                    permission?.UserPermissions.Add(new UserPermission
                    {
                        UserId = user.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            if (groups is not null)
            {
                foreach (var groupName in groups)
                {
                    var group = await _context.Set<Group>().FirstOrDefaultAsync(o => o.Name == groupName);

                    if (group is not null)
                    {
                        group.UserGroups.Add(new UserGroup
                        {
                            UserId = user.Id,
                            GroupId = group.Id
                        });

                        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.BelongsToGroup, group.Id.ToString()));
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task<bool> SeedUser(UserDTO userDTO)
    {
        var isCreated = false;
        var user = await _userManager.FindByEmailAsync(userDTO.Email);

        if (user is null)
        {
            user = _mapper.Map<User>(userDTO);
            user.UserName = userDTO.Email;
            user.EmailConfirmed = true;
            user.AccountStatus = AccountStatus.Active;

            var newPassword = _securityService.GetHashedPassword(userDTO.Password);
            var result = await _userManager.CreateAsync(user, newPassword);

            if (result.Succeeded)
            {
                await _securityService.SavePasswordToHistory(user);
                isCreated = true;
            }
        }

        return isCreated;
    }
}
