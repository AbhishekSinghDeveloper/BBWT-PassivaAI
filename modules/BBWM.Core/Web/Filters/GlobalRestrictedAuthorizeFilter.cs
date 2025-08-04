using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BBWM.Core.Web.Filters;

/// <summary>
/// Overrides standard <see cref="AuthorizeFilter"/> to provide the ability to avoid filter execution when specified attributes was set for end point.
/// Execution will not be canceled if <see cref="AuthorizeAttribute"/> was directly set for the end point.
/// </summary>
public class GlobalRestrictedAuthorizeFilter : AuthorizeFilter
{
    /// <summary>
    /// Initialize a new <see cref="GlobalRestrictedAuthorizeFilter"/> instance.
    /// </summary>
    public GlobalRestrictedAuthorizeFilter() : base() { }

    /// <summary>
    /// Initialize a new <see cref="GlobalRestrictedAuthorizeFilter"/> instance.
    /// </summary>
    /// <param name="policy">Authorization policy to be used.</param>
    public GlobalRestrictedAuthorizeFilter(AuthorizationPolicy policy) : base(policy) { }

    /// <summary>
    /// Initialize a new <see cref="GlobalRestrictedAuthorizeFilter"/> instance.
    /// </summary>
    /// <param name="policy">Authorization policy to be used.</param>
    /// <param name="restrictingTypes">Attributes types collection that avoids filter execution.</param>
    public GlobalRestrictedAuthorizeFilter(AuthorizationPolicy policy, IEnumerable<Type> restrictingTypes) : this(policy) =>
        RestrictingTypes = restrictingTypes;


    /// <summary>
    /// Attributes types collection that avoids filter execution.
    /// </summary>
    public IEnumerable<Type> RestrictingTypes { get; set; }


    public override async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any() &&
            RestrictingTypes.Intersect(context.ActionDescriptor.EndpointMetadata.Select(item => item.GetType())).Any())
            return;

        await base.OnAuthorizationAsync(context);
    }
}
