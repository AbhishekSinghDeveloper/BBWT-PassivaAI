using System.Security.Claims;
using BBF.Reporting.Core.Interfaces;
using BBWM.Core;
using Microsoft.AspNetCore.Http;
using ClaimTypes = System.Security.Claims.ClaimTypes;

namespace BBF.Reporting.Core.Services;

public class LoggedUserService : ILoggedUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggedUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public string? GetLoggedUserId()
        => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool IsSystemAdmin()
        => _httpContextAccessor.HttpContext?.User.IsInRole(Roles.SystemAdminRole) ?? false;
}