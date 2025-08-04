using BBWM.Core.Data;
using BBWM.Core.Membership.Constants;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Web.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using UAParser.FormFactor;

namespace BBWM.Core.Membership.Services;

public class LoginAuditService : ILoginAuditService
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<LoginAuditService> _logger;
    private readonly IDbContext _context;

    public LoginAuditService(
        IHttpContextAccessor contextAccessor,
        ILogger<LoginAuditService> logger,
        IDbContext context)
    {
        _context = context;
        _contextAccessor = contextAccessor;
        _logger = logger;
    }

    public Task<int> GetLastAttemptsCountAsync(string ip, DateTimeOffset withInDate) =>
        _context.Set<LoginAudit>().CountAsync(c => c.Ip == ip && c.Datetime > withInDate);

    public Task<LoginAudit> GetLastSuccessfulLoginAuditAsync(string userEmail)
    {
        return _context.Set<LoginAudit>()
            .AsNoTracking()
            .Where(x => x.Email == userEmail && x.Result.Contains(LogMessages.LoggedIn))
            .OrderByDescending(x => x.Datetime)
            .FirstOrDefaultAsync();
    }

    public Task<LoginAudit> GetLastSignedOutAuditAsync(string userEmail)
    {
        return _context.Set<LoginAudit>()
            .AsNoTracking()
            .Where(x => x.Email == userEmail && x.Result.Contains(LogMessages.SignedOut))
            .OrderByDescending(x => x.Datetime)
            .FirstOrDefaultAsync();
    }

    public Task<LoginAudit> GetLastPassed2FACodeAuditAsync(string userEmail)
    {
        return _context.Set<LoginAudit>()
            .AsNoTracking()
            .Where(x => x.Email == userEmail && x.Result.Contains(LogMessages.Passed2FACode))
            .OrderByDescending(x => x.Datetime)
            .FirstOrDefaultAsync();
    }

    public async Task SaveLoginAuditAsync(User user, string result)
    {
        var httpContext = _contextAccessor.HttpContext;
        var browserId = httpContext.Request.Headers["X-Browser-Id"].ToString();
        var browserFingerprint = httpContext.Request.Headers["X-Browser-Fingerprint"].ToString();
        var ip = httpContext.GetUserIp();

        if (string.IsNullOrEmpty(browserId))
            browserId = Parser.GetDefault().Parse(httpContext.Request.Headers["User-Agent"])?.UA.ToString();

        await _context.Set<LoginAudit>().AddAsync(
            new LoginAudit()
            {
                Datetime = DateTimeOffset.Now,
                Email = user?.Email,
                Ip = ip,
                Browser = browserId,
                Fingerprint = browserFingerprint,
                Location = null,
                Result = result
            });
        await _context.SaveChangesAsync();

        var logMessage = user is null
            ? $"User 'Unknown' with Ip {ip} {result}"
            : $"User '{user.Email}' with ID {user.Id} {result}";

        _logger.LogInformation(logMessage);
    }
}
