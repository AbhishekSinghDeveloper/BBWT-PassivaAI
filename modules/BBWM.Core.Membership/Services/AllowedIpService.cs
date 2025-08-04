using BBWM.Core.Data;
using BBWM.Core.Membership.Exceptions;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Membership.Services;

public class AllowedIpService : IAllowedIpService
{
    private readonly IDbContext _context;
    private readonly UserManager<User> _userManager;

    public AllowedIpService(
        IDbContext context,
        UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<bool> IsServiceActive(CancellationToken ct = default)
        => await _context.Set<AllowedIpRole>().AnyAsync(ct) || await _context.Set<AllowedIpUser>().AnyAsync(ct);


    public async Task<bool> IsIpAllowedForUser(string ip, string userId, CancellationToken cancellationToken = default)
    {
        if (ip == "0.0.0.1") return true; // localhost

        var user = await _userManager.Users
            .Include(u => u.AllowedIpUser)
            .ThenInclude(al => al.AllowedIp)
            .Include(u => u.UserRoles)
            .ThenInclude(us => us.Role)
            .ThenInclude(r => r.AllowedIpRoles)
            .ThenInclude(ipr => ipr.AllowedIp)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null) throw new UserNotExistsException();

        // Users with SuperAdmin role is excluded from the allowed IPs filtration in order not to block the whole site's UI.
        // At least SuperAdmin user should be able to log in.
        if (user.UserRoles.Any(x => x.Role.Name == Roles.SuperAdminRole))
            return true;

        var allowedUserIpRanges = user.AllowedIpUser
            .Select(x => x.AllowedIp)
            .ToArray();

        var allowedUserRolesIpRanges = user.UserRoles
            .Select(x => x.Role)
            .SelectMany(x => x.AllowedIpRoles)
            .Select(x => x.AllowedIp)
            .Distinct()
            .ToArray();

        // If the user or his roles has restrictions of IP ranges then he must be checked that his IP match at least one range
        if (allowedUserIpRanges.Any() || allowedUserRolesIpRanges.Any())
        {
            if (allowedUserIpRanges.Any(allowedIp => IsIpInRange(allowedIp.IpAddressFirst, allowedIp.IpAddressLast, ip)))
                return true;

            if (allowedUserRolesIpRanges.Any(allowedIp => IsIpInRange(allowedIp.IpAddressFirst, allowedIp.IpAddressLast, ip)))
                return true;

            return false;
        }

        return true;
    }

    private static long IpToLong(string ip)
    {
        double num = 0;
        if (string.IsNullOrEmpty(ip)) return (long)num;

        var ipBytes = ip.Split('.');
        for (var i = ipBytes.Length - 1; i >= 0; i--)
        {
            num += int.Parse(ipBytes[i]) % 256 * Math.Pow(256, 3 - i);
        }
        return (long)num;
    }

    private static bool IsIpInRange(string lowRange, string highRange, string ipAddress)
    {
        var lowerRange = IpToLong(lowRange);
        var upperRange = IpToLong(highRange);
        var ipAddressLong = IpToLong(ipAddress);

        return ipAddressLong >= lowerRange && ipAddressLong <= upperRange;
    }
}
