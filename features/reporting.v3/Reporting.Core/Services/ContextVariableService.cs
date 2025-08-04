using BBF.Reporting.Core.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBWM.Core.Web.Extensions;
using Microsoft.AspNetCore.Http;

namespace BBF.Reporting.Core.Services;

public class ContextVariableService : IContextVariableService
{
    private const string UserEmailVariableName = "user_email";
    private const string UserIdVariableName = "user_id";
    private const string UserOrgIdVariableName = "user_org_id";
    private const string UserOrgsIdListVariableName = "user_orgs_id_list";

    private readonly IDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;


    public ContextVariableService(IDbContext context, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public string? GetVariableValue(string variableName) => variableName switch
    {
        UserIdVariableName => GetUserId() is { } userId ? $"'{userId}'" : null,
        UserEmailVariableName => GetUserEmail() is { } email ? $"'{email}'" : null,
        UserOrgIdVariableName => GetUserOrganizationId() is { } organizationId ? $"{organizationId}" : null,
        UserOrgsIdListVariableName => GetUserOrganizations() is { Count: > 0 } organizationIds ? $"({string.Join(", ", organizationIds)})" : null,
        _ => throw new ArgumentException("An unsupported variable name was specified."),
    };

    public IEnumerable<string> GetVariableNames() => new[]
    {
        UserEmailVariableName,
        UserIdVariableName,
        UserOrgIdVariableName,
        UserOrgsIdListVariableName
    };

    private string GetUserEmail() => _contextAccessor.HttpContext.GetUserEmail();

    private string GetUserId() => _contextAccessor.HttpContext.GetUserId();

    private int? GetUserOrganizationId()
    {
        var userId = GetUserId();

        return _context.Set<User>()
            .Where(x => x.Id == userId)
            .Select(x => x.OrganizationId)
            .FirstOrDefault();
    }

    private IList<int> GetUserOrganizations()
    {
        var userId = GetUserId();

        // Get organization ids corresponding to this user.
        return _context.Set<UserOrganization>()
            .Where(userOrganization => userOrganization.UserId == userId)
            .Select(userOrganization => userOrganization.OrganizationId)
            .ToList();
    }
}