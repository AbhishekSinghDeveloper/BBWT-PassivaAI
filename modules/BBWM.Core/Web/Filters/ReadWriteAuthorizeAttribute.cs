using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BBWM.Core.Web.Filters;

/// <summary>
/// Specifies that the controller that this attribute is applied to requires the specified roles for reading and writing actions.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ReadWriteAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // If the action has its own AuthorizeAttribute we skipping check.
        if (context.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any() ||
            context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            return;

        switch (context.HttpContext.Request.Method.ToUpperInvariant())
        {
            case "GET": CheckRoles(context, SplitRoles(ReadWriteRoles ?? ReadRoles)); break;
            case "POST": case "PUT": case "PATCH": case "DELETE": CheckRoles(context, SplitRoles(ReadWriteRoles ?? WriteRoles)); break;
            default: return; // The rest of methods should not be affected by check.
        }
    }


    /// <summary>
    /// Gets or sets a comma delimited list of roles that are allowed to access read end points.
    /// </summary>
    /// <remarks>
    /// Default value is "NoOne".
    /// </remarks>
    public string ReadRoles { get; set; } = AggregatedRoles.Noone;

    /// <summary>
    /// Gets or sets a comma delimited list of roles that are allowed to access write end points.
    /// </summary>
    /// <remarks>
    /// Default value is "NoOne".
    /// </remarks>
    public string WriteRoles { get; set; } = AggregatedRoles.Noone;

    /// <summary>
    /// Gets or sets a comma delimited list of roles that are allowed to access both write and read end points.
    /// This property are more prior then "ReadRoles" and "WriteRoles".
    /// </summary>
    public string ReadWriteRoles { get; set; }


    private static List<string> SplitRoles(string roles) =>
        roles?.Split(",").Select(roleItem => roleItem.Trim()).ToList() ?? new List<string>();

    private static void SetForbidden(AuthorizationFilterContext context) =>
        context.Result = new ForbidResult();

    private static void CheckRoles(AuthorizationFilterContext context, List<string> roles)
    {
        if (roles.Contains(AggregatedRoles.Anyone)) return;

        if (roles.Contains(AggregatedRoles.Noone))
        {
            SetForbidden(context);
            return;
        }

        if (roles.Contains(AggregatedRoles.Authenticated))
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
                context.Result = new UnauthorizedResult();

            return;
        }


        if (roles.Any(roleItem => context.HttpContext.User.IsInRole(roleItem))) return;
        SetForbidden(context);
    }
}
