using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Utils;
using BBWM.Core.Utils;
using BBWM.GitLab;
using BBWM.Metadata;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System.Text.Json;

namespace BBWM.Core.Membership.Services;

public class RoleGitDataService : IRoleGitDataService
{
    private const string RolesMetadataKey = "ROLES";
    private readonly IDbContext _context;
    private readonly IMetadataService _metadataService;
    private readonly IGitLabService _gitlabService;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager;
    private readonly string _rolesJsonPath;

    public RoleGitDataService(
        IDbContext context,
        IWebHostEnvironment hostingEnvironment,
        IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager,
        IMetadataService metadataService,
        IGitLabService gitlabService,
        IOptionsSnapshot<MembershipSettings> options)
    {
        _context = context;
        _hostingEnvironment = hostingEnvironment;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _metadataService = metadataService;
        _gitlabService = gitlabService;
        _rolesJsonPath = $"{_hostingEnvironment.ContentRootPath}{options.Value.RolesFilePath}";
    }

    // TODO: then it should become a part of new BBWM.GitData (ref BBWM.GitLab, BBWM.Metadata)
    public async Task UpdateRolesFromJson(CancellationToken cancellationToken = default)
    {
        RolesGitDataDTO rolesGitData;
        try
        {
            var json = File.ReadAllText(_rolesJsonPath);
            rolesGitData = string.IsNullOrEmpty(json) ? null :
                JsonSerializer.Deserialize<RolesGitDataDTO>(json, JsonSerializerOptionsProvider.Options);
        }
        catch (Exception)
        {
            rolesGitData = null;
        }

        var metadata = _metadataService.GetByKey(RolesMetadataKey);

        if (rolesGitData is not null &&
            (metadata?.LastUpdated is null || rolesGitData.LastUpdated > metadata.LastUpdated))
        {
            await UpdateRolesFromMetadataObject(rolesGitData.Roles, cancellationToken);
            UpdateMetadata(rolesGitData.Roles);
        }
    }

    public async Task SendToGit(CancellationToken cancellationToken = default)
    {
        var rolesMetadata = await GetMetadataObjectFromRoles(cancellationToken);

        // Save to metadata
        var metadataDto = UpdateMetadata(rolesMetadata);

        var gitData = new RolesGitDataDTO
        {
            LastUpdated = metadataDto.LastUpdated,
            Roles = rolesMetadata
        };

        var json = JsonSerializer.Serialize(gitData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        // We check that it's not a local environment. JSON should be send from live/test site only and then it's being used
        // to update roles list of all site deployments (including local ones)
        if (!_hostingEnvironment.IsDevelopment())
        {
            // Send to Git
            var email = (await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User))?.Email;
            if (string.IsNullOrEmpty(email))
                throw new ConflictException("User's email address not found.");

            await _gitlabService.Push("data/roles/roles", json, email, cancellationToken);
        }
    }

    private async Task UpdateRolesFromMetadataObject(ICollection<RoleMetadataDTO> rolesMetadata,
        CancellationToken ct)
    {
        var existentRoles = await GetRoles(ct);
        var rolesToRemove = new List<Role>();

        foreach (var existingRole in existentRoles)
        {
            if (!rolesMetadata.Any(x => x.Id == existingRole.Id))
            {
                if (await _context.Set<UserRole>().AnyAsync(o => o.RoleId == existingRole.Id, ct))
                    throw new ConflictException($"Role (ID: {existingRole.Id}, name: {existingRole.Name}) is still in use. " +
                        $"Please remove all users from this user-role before deleting it.");

                _context.Set<Role>().Remove(existingRole);
                rolesToRemove.Add(existingRole);
            }
        }

        var updatedRoles = existentRoles.Except(rolesToRemove).ToList();

        foreach (var roleMetadata in rolesMetadata)
        {
            if (updatedRoles.Any(o => o.Id != roleMetadata.Id && o.Name.Equals(roleMetadata.Name, StringComparison.InvariantCultureIgnoreCase)))
                throw new ConflictException($"Role name '{roleMetadata.Name}' already exists.");

            var role = updatedRoles.SingleOrDefault(o => o.Id == roleMetadata.Id);
            await UpdateDbRoleByRoleMetadata(role, roleMetadata, ct);
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task UpdateDbRoleByRoleMetadata(
        Role role,
        RoleMetadataDTO roleMetadata,
        CancellationToken ct)
    {
        var isNewRole = role is null;

        if (isNewRole)
        {
            role = new Role { Id = roleMetadata.Id };
        }

        role.Name = roleMetadata.Name;
        role.NormalizedName = roleMetadata.Name.ToUpperInvariant();
        role.ConcurrencyStamp = Guid.NewGuid().ToString();

        if (isNewRole)
        {
            _context.Set<Role>().Add(role);
        }
        else
        {
            _context.Set<Role>().Update(role);
        }

        #region replace permissions of the role

        _context.Set<RolePermission>().RemoveRange(role.RolePermissions.Where(x =>
            roleMetadata.Permissions.All(y =>
                !y.Equals(x.Permission.Name, StringComparison.InvariantCultureIgnoreCase))));

        foreach (var permissionName in roleMetadata.Permissions)
        {
            var permissionId = await _context.Set<Permission>()
                .Where(o => o.Name == permissionName)
                .Select(o => o.Id)
                .SingleOrDefaultAsync(ct);

            if (permissionId == default)
                throw new ConflictException($"Permission '{permissionName}' is not found in database.");

            if (role.RolePermissions.All(o => o.PermissionId != permissionId))
            {
                _context.Set<RolePermission>().Add(
                    new RolePermission { RoleId = roleMetadata.Id, PermissionId = permissionId });
            }
        }

        #endregion replace permissions of the role
    }

    private MetadataDTO UpdateMetadata(List<RoleMetadataDTO> rolesMetadata)
    {
        var metadata = _metadataService.GetByKey(RolesMetadataKey) ?? new MetadataDTO { Key = RolesMetadataKey };
        metadata.Value = JsonSerializer.Serialize(rolesMetadata);
        return _metadataService.Save(metadata);
    }

    private async Task<List<RoleMetadataDTO>> GetMetadataObjectFromRoles(CancellationToken cancellationToken) =>
        (await GetRoles(cancellationToken)).ConvertAll(r =>
                new RoleMetadataDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Permissions = r.RolePermissions.Select(rp => rp.Permission.Name).ToList()
                });

    private async Task<List<Role>> GetRoles(CancellationToken cancellationToken)
    {
        var coreRoles = RolesExtractor.GetAllRolesNamesOfSolution();

        return await _context.Set<Role>()
            .Where(x => !coreRoles.Contains(x.Name))
            .Include(x => x.RolePermissions).ThenInclude(x => x.Permission)
            .ToListAsync(cancellationToken);
    }
}