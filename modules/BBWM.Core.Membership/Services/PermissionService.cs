using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Utils;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Membership.Services;

public class PermissionService : IPermissionService
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;


    public PermissionService(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ICollection<PermissionDTO>> GetAll(CancellationToken cancellationToken = default) =>
        _mapper.Map<ICollection<PermissionDTO>>(await _context.Set<Permission>().ToListAsync(cancellationToken));

    public async Task CleanupPermissions(CancellationToken cancellationToken = default)
    {
        var exceptPermissions = PermissionsExtractor.GetAllPermissionNamesOfSolution().ToArray();
        var permissions = await _context.Set<Permission>().ToListAsync(cancellationToken);

        foreach (var permission in permissions.Where(permission => exceptPermissions.All(o => o != permission.Name)))
        {
            _context.Set<Permission>().Remove(permission);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

}
