using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Utils;
using BBWM.Core.ModelHashing;
using BBWM.Core.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Membership.Services;

public class RoleService : IRoleService
{
    private IMapper Mapper { get; }
    private IDbContext Context { get; }

    private readonly RoleManager<Role> _roleManager;
    private readonly IModelHashingService _modelHashingService;
    private readonly IDataService _dataService;

    public RoleService(
        IDbContext context,
        IMapper mapper,
        IDataService dataService,
        RoleManager<Role> roleManager,
        IModelHashingService modelHashingService)
    {
        Context = context;
        Mapper = mapper;
        _dataService = dataService;
        _roleManager = roleManager;
        _modelHashingService = modelHashingService;
    }

    public IQueryable<Role> GetEntityQuery(IQueryable<Role> baseQuery)
    {
        var coreRoles = RolesExtractor.GetAllRolesNamesOfSolution();
        return baseQuery
            .Include(x => x.RolePermissions).ThenInclude(x => x.Permission)
            .Where(x => !coreRoles.Contains(x.Name));
    }

    public IEnumerable<RoleDTO> GetHardcodedRoles()
    {
        var coreRoles = RolesExtractor.GetAllRolesNamesOfSolution();
        return Mapper.Map<IEnumerable<RoleDTO>>(_roleManager.Roles.Where(x => coreRoles.Contains(x.Name)));
    }

    public IEnumerable<RoleDTO> GetProjectRoles() => Mapper.Map<IEnumerable<RoleDTO>>(GetEntityQuery(Context.Set<Role>()));


    public async Task<RoleDTO> Create(RoleDTO dto, CancellationToken ct)
    {
        if (await _roleManager.FindByNameAsync(dto.Name) is not null)
            throw new BusinessException("Role name already exists.");

        var role = Mapper.Map<Role>(dto);
        role.Id = Guid.NewGuid().ToString();
        await _roleManager.CreateAsync(role);

        await ReplacePermissionsForRole(role.Id, dto.Permissions, ct);
        return Mapper.Map<RoleDTO>(role);
    }

    public async Task<RoleDTO> Update(RoleDTO dto, CancellationToken ct)
    {
        var existingRoleWithSuchName = await _roleManager.FindByNameAsync(dto.Name);
        if (existingRoleWithSuchName is not null && existingRoleWithSuchName.Id != dto.Id)
            throw new BusinessException("Role name already exists.");

        var role = await _roleManager.FindByIdAsync(dto.Id);
        if (role is null)
            throw new ObjectNotExistsException("Role not found.");

        role.Name = dto.Name;
        role.AuthenticatorRequired = dto.AuthenticatorRequired;
        await _roleManager.UpdateAsync(role);

        await ReplacePermissionsForRole(role.Id, dto.Permissions, ct);
        return Mapper.Map<RoleDTO>(role);
    }

    public async Task Delete(string id, CancellationToken ct)
    {
        if (await Context.Set<UserRole>().AnyAsync(o => o.RoleId == id, ct))
            throw new BusinessException("The role is still in use. Please remove all users from this user-role before deleting it.");

        var role = await _roleManager.FindByIdAsync(id);

        if (role is null)
            throw new ObjectNotExistsException("Role not found.");

        await _roleManager.DeleteAsync(role);
    }

    public async Task CleanupRoles(CancellationToken cancellationToken = default)
    {
        var exceptRoles = RolesExtractor.GetAllRolesNamesOfSolution().ToArray();
        var roles = await _roleManager.Roles.ToListAsync(cancellationToken);

        foreach (var role in roles.Where(role => exceptRoles.All(o => o != role.Name)))
        {
            await _roleManager.DeleteAsync(role);
        }
    }

    private async Task ReplacePermissionsForRole(string roleId, ICollection<PermissionDTO> newPermissionsSet, CancellationToken cancellationToken = default)
    {
        newPermissionsSet ??= new List<PermissionDTO>();

        var role = await _roleManager.Roles
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);

        if (role is null) throw new ObjectNotExistsException("Role doesn't exist.");

        Context.Set<RolePermission>().RemoveRange(
            role.RolePermissions.Where(x => newPermissionsSet.All(y => y.Id != x.PermissionId)));

        await Context.Set<RolePermission>().AddRangeAsync(
            newPermissionsSet
                .Where(x => role.RolePermissions.All(y => y.PermissionId != x.Id))
                .Select(x => new RolePermission { RoleId = roleId, PermissionId = x.Id }),
            cancellationToken);

        await Context.SaveChangesAsync(cancellationToken);
    }

    public Task<PageResult<RoleDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
        => _dataService.GetPage<Role, RoleDTO>(command, GetEntityQuery,
            queryFilter => queryFilter
                .Handle("permissions", (query, filter) =>
                {
                    var unHashedPermissionId = _modelHashingService.UnHashProperty<PermissionDTO>(nameof(PermissionDTO.Id), filter.Value);
                    return query.Where(x => x.RolePermissions.Any(y => y.PermissionId == unHashedPermissionId));
                }),
            ct: ct);
}
