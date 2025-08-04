using BBWM.Core.Data;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Membership.Services;

public interface IAllowedIpDataService :
    IEntityQuery<AllowedIp>,
    IEntityCreate<AllowedIpDTO>,
    IEntityUpdate<AllowedIpDTO>
{
}

public class AllowedIpDataService : IAllowedIpDataService
{
    private readonly IDbContext _context;
    private readonly IDataService _dataService;

    public AllowedIpDataService(IDbContext context, IDataService dataService)
    {
        _context = context;
        _dataService = dataService;
    }

    public IQueryable<AllowedIp> GetEntityQuery(IQueryable<AllowedIp> baseQuery)
        => baseQuery
            .Include(x => x.AllowedIpRoles)
                .ThenInclude(x => x.Role)
            .Include(x => x.AllowedIpUsers)
                .ThenInclude(x => x.User);

    public Task<AllowedIpDTO> Create(AllowedIpDTO dto, CancellationToken ct = default)
        => _dataService.Create<AllowedIp, AllowedIpDTO>(dto, (entity, ctx) => BeforeAllowedIpSave(dto, entity), ct);

    public Task<AllowedIpDTO> Update(AllowedIpDTO dto, CancellationToken ct = default)
        => _dataService.Update<AllowedIp, AllowedIpDTO>(dto, (entity, ctx) => BeforeAllowedIpSave(dto, entity), ct);

    private void BeforeAllowedIpSave(AllowedIpDTO dto, AllowedIp entity)
    {
        UpdateAllowedIpRole(entity, dto.Roles);
        UpdateAllowedIpUser(entity, dto.Users);
    }

    private void UpdateAllowedIpRole(AllowedIp allowedIp, IList<RoleDTO> newRoles)
    {
        if (newRoles is null) return;

        var oldRoles = _context.Set<AllowedIpRole>().Where(x => x.AllowedIpId == allowedIp.Id);
        foreach (var roleDto in newRoles)
        {
            if (oldRoles.Select(x => x.RoleId).Contains(roleDto.Id)) continue;

            var allowedIpRole = new AllowedIpRole
            {
                RoleId = roleDto.Id,
                AllowedIp = allowedIp
            };

            _context.Set<AllowedIpRole>().Add(allowedIpRole);
        }

        foreach (var role in oldRoles)
        {
            if (newRoles.All(x => x.Id != role.RoleId))
            {
                _context.Set<AllowedIpRole>().Remove(role);
            }
        }
    }

    private void UpdateAllowedIpUser(AllowedIp allowedIp, IList<UserDTO> newUsers)
    {
        if (newUsers is null) return;

        var oldUsers = _context.Set<AllowedIpUser>().Where(x => x.AllowedIpId == allowedIp.Id);
        allowedIp.AllowedIpUsers = new List<AllowedIpUser>();
        foreach (var userDto in newUsers)
        {
            if (oldUsers.Select(x => x.UserId).Contains(userDto.Id)) continue;

            var allowedIpUser = new AllowedIpUser
            {
                UserId = userDto.Id,
                AllowedIp = allowedIp
            };

            _context.Set<AllowedIpUser>().Add(allowedIpUser);
        }

        foreach (var user in oldUsers)
        {
            if (newUsers.All(x => x.Id != user.UserId))
            {
                _context.Set<AllowedIpUser>().Remove(user);
            }
        }
    }
}
