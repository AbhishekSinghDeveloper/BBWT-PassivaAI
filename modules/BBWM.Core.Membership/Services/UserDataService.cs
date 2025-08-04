using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Extensions;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Enums;
using BBWM.Core.Membership.Exceptions;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.ModelHashing;
using BBWM.Core.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using ClaimTypes = BBWM.Core.Membership.Model.ClaimTypes;

namespace BBWM.Core.Membership.Services;

public class UserDataService : IUserDataService
{
    private readonly IDataService _dataService;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IModelHashingService _modelHashingService;
    private readonly IMapper _mapper;
    private readonly IDbContext _context;

    public UserDataService(
        IDataService dataService,
        IModelHashingService modelHashingService,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
        _dataService = dataService;
        _modelHashingService = modelHashingService;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IQueryable<User> GetEntityQuery(IQueryable<User> baseQuery)
        => _userManager.Users
            .Include(x => x.AuthenticationRequests)
            .Include(x => x.AvatarImage)
            .Include(x => x.UserGroups).ThenInclude(x => x.Group)
            .Include(x => x.UserOrganizations).ThenInclude(x => x.Organization)
            .Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .Include(x => x.UserPermissions).ThenInclude(x => x.Permission)
            .Include(x => x.Organization).ThenInclude(x => x.Branding).ThenInclude(x => x.LogoImage)
            .Include(x => x.Organization).ThenInclude(x => x.Branding).ThenInclude(x => x.LogoIcon)
            .Include(x => x.DeviceRegistrations)
            .Include(x => x.EmailConfirmationToken)
            .Include(x => x.InvitationToken)
            .Include(x => x.PasswordResetToken);

    public Task<UserDTO> Get(string id, CancellationToken ct = default)
        => _dataService.Get<User, UserDTO, string>(id, GetEntityQuery, ct);

    public Task<PageResult<UserDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
        => _dataService.GetPage<User, UserDTO>(command, GetEntityQuery,
            queryFilter => queryFilter
                .Handle<NumberArrayFilter>("organizationId", (query, filter) =>
                    filter.Value is null ?
                        query
                        : query.Where(x => x.UserOrganizations.Any(y => filter.Value.Contains(y.OrganizationId))))

                .Handle("roles", (query, filter) =>
                    query.Where(x => x.UserRoles.Any(y => y.RoleId == filter.Value))),
            ct: ct);

    public async Task<bool> Exists(string id, CancellationToken cancellationToken = default)
        => await _userManager.FindByIdAsync(id) is not null;

    public Task<UserDTO> GetByEmail(string email, CancellationToken cancellationToken = default) =>
        _dataService.Get<User, UserDTO>(query => query.Where(o => o.Email == email), cancellationToken);

    public async Task<UserDTO> Create(UserDTO dto, CancellationToken ct = default)
    {
        dto = BeforeUserSave(dto);

        var user = _mapper.Map<User>(dto);
        await _userManager.CreateAsync(user);

        await AfterUserSave(user, dto, ct);

        return await Get(user.Id, ct);
    }

    public async Task<UserDTO> Update(UserDTO dto, CancellationToken ct = default)
    {
        dto = BeforeUserSave(dto);

        var user = await _userManager.FindByIdAsync(dto.Id);

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;
        user.UserName = dto.Email;
        user.GravatarImage = dto.GravatarImage;
        user.GravatarEmail = dto.GravatarEmail;
        user.NormalizedEmail = dto.Email.ToUpperInvariant();
        user.NormalizedUserName = dto.Email.ToUpperInvariant();
        user.OrganizationId = dto.OrganizationId;
        user.TwoFactorEnabled = dto.TwoFactorEnabled;
        user.U2fEnabled = dto.U2fEnabled;
        // user.RecoveryCode = dto.RecoveryCode;
        user.AvatarImageId = dto.AvatarImageId;
        user.PictureMode = dto.PictureMode;

        await _userManager.UpdateAsync(user);

        await AfterUserSave(user, dto, ct);

        return await Get(user.Id, ct);
    }

    public async Task<ICollection<UserDTO>> ReplaceUsersRoles(UsersRolesReplacementDTO dto, CancellationToken cancellationToken = default)
    {
        foreach (var userId in dto.UsersIds)
        {
            var originalUser = await _userManager.Users
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (originalUser is null)
                throw new UserNotExistsException(ErrorMessages.UserNotExistForId.Replace("userId", userId));

            await UpdateUserRoles(
                originalUser,
                originalUser.UserRoles.Select(x => x.RoleId)
                    .Union(dto.RolesIdsToAdd)
                    .Where(x => dto.RolesIdsToRemove.All(y => y != x)),
                cancellationToken);
        }

        var users = await GetEntityQuery(_userManager.Users).Where(x => dto.UsersIds.Contains(x.Id))
                .ToListAsync(cancellationToken);
        return _mapper.Map<ICollection<UserDTO>>(users);
    }

    public async Task<ICollection<UserDTO>> ReplaceUsersGroups(UsersGroupsReplacementDTO dto, CancellationToken cancellationToken = default)
    {
        foreach (var userId in dto.UsersIds)
        {
            var originalUser = await _userManager.Users
                .Include(x => x.UserGroups)
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (originalUser is null)
                throw new UserNotExistsException(ErrorMessages.UserNotExistForId.Replace("userId", userId));

            await UpdateUserGroups(
                originalUser,
                originalUser.UserGroups.Select(x => x.GroupId)
                    .Union(dto.GroupsIdsToAdd.Select(x => _modelHashingService.UnHashProperty<GroupDTO>(nameof(GroupDTO.Id), x)).Cast<int>())
                    .Where(x => dto.GroupsIdsToRemove.All(y => _modelHashingService.UnHashProperty<GroupDTO>(nameof(GroupDTO.Id), y) != x)),
                cancellationToken);
        }

        var users = await GetEntityQuery(_userManager.Users).Where(x => dto.UsersIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
        return _mapper.Map<ICollection<UserDTO>>(users);
    }

    private static UserDTO BeforeUserSave(UserDTO dto)
    {
        dto.Email = dto.Email.Trim();
        dto.UserName = dto.UserName?.Trim();
        dto.FirstName = dto.FirstName?.Trim();
        dto.LastName = dto.LastName?.Trim();
        dto.PhoneNumber = dto.PhoneNumber?.Trim();
        return dto;
    }

    private async Task AfterUserSave(User user, UserDTO dto, CancellationToken cancellationToken)
    {
        await UpdateUserRoles(user, dto.Roles.Select(x => x.Id), cancellationToken);
        await UpdateUserPermissions(user, dto.Permissions.Select(x => x.Id), cancellationToken);
        await UpdateUserGroups(user, dto.Groups.Select(x => x.Id), cancellationToken);

        #region Organizations
        // As user.Organization ID is a default organization whereas all list of user organizations is stored
        // in UserOrganizations, then we set default organization when not defined.

        if (!dto.Organizations.Any())
        {
            user.OrganizationId = null;
            await _userManager.UpdateAsync(user);
        }
        else if (dto.OrganizationId is null || !dto.Organizations.Any(x => x.Id == dto.OrganizationId))
        {

            user.OrganizationId = dto.Organizations.First().Id;
            await _userManager.UpdateAsync(user);
        }


        await UpdateUserOrganizations(user, dto.Organizations.Select(x => x.Id), cancellationToken);
        #endregion
    }

    public async Task Delete(string id, CancellationToken ct = default)
    {
        //https://entityframework.net/knowledge-base/24271715/mvc5-ef6--object-cannot-be-deleted-because-it-was-not-found-in-the-objectstatemanager-
        var user = await _userManager.FindByIdAsync(id);

        if (!_userManager.Users.Any(user => user.Id == id))
            throw new UserNotExistsException();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new BusinessException("User was not deleted");
    }

    public Task<IEnumerable<RoleDTO>> GetAllRoles(CancellationToken cancellationToken = default)
        => _dataService.GetAll<Role, RoleDTO>(query =>
            query.Include(x => x.RolePermissions).ThenInclude(x => x.Permission), cancellationToken);

    public Task<IEnumerable<GroupDTO>> GetAllGroups(CancellationToken cancellationToken = default)
        => _dataService.GetAll<Group, GroupDTO>(cancellationToken);

    public Dictionary<string, object> GetAllAccountStatuses(CancellationToken cancellationToken = default) =>
        typeof(AccountStatus).GetEnumNamesValues();

    private async Task UpdateUserRoles(User user, IEnumerable<string> newRolesIds, CancellationToken cancellationToken)
    {
        newRolesIds = newRolesIds.ToList();
        var existingRoles = _context.Set<UserRole>()
            .Include(x => x.Role)
            .Where(x => x.UserId == user.Id);

        foreach (var existingRole in existingRoles)
        {
            if (newRolesIds.All(x => x != existingRole.RoleId))
            {
                await _userManager.RemoveFromRoleAsync(user, existingRole.Role.Name);
            }
        }

        foreach (var newRoleId in newRolesIds)
        {
            if (await existingRoles.AllAsync(x => x.RoleId != newRoleId, cancellationToken))
            {
                var role = await _roleManager.FindByIdAsync(newRoleId);
                await _userManager.AddToRoleAsync(user, role.Name);
            }
        }
    }

    private async Task UpdateUserPermissions(User user, IEnumerable<int> newPermissionsIds, CancellationToken cancellationToken)
    {
        newPermissionsIds = newPermissionsIds.ToList();
        var existingPermissions = _context.Set<UserPermission>().Where(x => x.UserId == user.Id);

        foreach (var existingPermission in existingPermissions)
        {
            if (newPermissionsIds.All(x => x != existingPermission.PermissionId))
            {
                _context.Set<UserPermission>().Remove(existingPermission);
            }
        }

        foreach (var newPermissionId in newPermissionsIds)
        {
            if (await existingPermissions.AllAsync(x => x.PermissionId != newPermissionId, cancellationToken))
            {
                await _context.Set<UserPermission>().AddAsync(
                        new UserPermission
                        {
                            UserId = user.Id,
                            PermissionId = newPermissionId
                        },
                        cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateUserGroups(User user, IEnumerable<int> newGroupsIds, CancellationToken cancellationToken)
    {
        newGroupsIds = newGroupsIds.ToList();
        var existingGroups = _context.Set<UserGroup>().Where(x => x.UserId == user.Id);
        var existingUsersGroupClaims =
            (await _userManager.GetClaimsAsync(user)).Where(x => x.Type == ClaimTypes.BelongsToGroup).ToList();

        foreach (var existingGroup in existingGroups)
        {
            if (newGroupsIds.All(x => x != existingGroup.GroupId))
            {
                _context.Set<UserGroup>().Remove(existingGroup);

                var userClaim = existingUsersGroupClaims.FirstOrDefault(x => x.Value == existingGroup.GroupId.ToString());
                if (userClaim is not null)
                    await _userManager.RemoveClaimAsync(user, userClaim);
            }
        }

        foreach (var newGroupId in newGroupsIds)
        {
            if (await existingGroups.AllAsync(x => x.GroupId != newGroupId, cancellationToken))
            {
                await _context.Set<UserGroup>().AddAsync(
                    new UserGroup
                    {
                        UserId = user.Id,
                        GroupId = newGroupId
                    },
                    cancellationToken);

                await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.BelongsToGroup, newGroupId.ToString()));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateUserOrganizations(User user, IEnumerable<int> newOrganizationIds, CancellationToken ct)
    {
        newOrganizationIds = newOrganizationIds.ToList();
        var existingItems = _context.Set<UserOrganization>().Where(x => x.UserId == user.Id);

        foreach (var existingItem in existingItems)
        {
            if (newOrganizationIds.All(x => x != existingItem.OrganizationId))
            {
                _context.Set<UserOrganization>().Remove(existingItem);
            }
        }

        foreach (var newItemId in newOrganizationIds)
        {
            if (await existingItems.AllAsync(x => x.OrganizationId != newItemId, ct))
            {
                await _context.Set<UserOrganization>().AddAsync(
                    new UserOrganization
                    {
                        UserId = user.Id,
                        OrganizationId = newItemId
                    },
                    ct);
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<UserSignatureDTO> GetUserSignature(string userId, CancellationToken cancellation = default)
    {
        var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            throw new EntityNotFoundException("User not found.");

        return _mapper.Map<UserSignatureDTO>(user);
    }

    public async Task<bool> SetUserSignature(string userId, string signature, CancellationToken cancellation = default)
    {
        var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            throw new EntityNotFoundException("User not found.");

        user.UserSignatureJson = signature;
        await _context.SaveChangesAsync();

        return true;
    }

    public static class ErrorMessages
    {
        public static readonly string UserNotExistForId = "User with id userId doesn't exist.";
    }
}
