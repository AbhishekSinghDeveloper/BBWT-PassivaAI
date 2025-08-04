using Microsoft.AspNetCore.Mvc.Filters;

namespace BBWM.Core.Web.CookieAuth;

///<summary>
/// Prevent the auth cookie from being reset for this action, allows you to
/// have requests that do not reset the sliding login timeout.
/// </summary>
public class DoNotResetAuthCookieAttribute : ActionFilterAttribute
{
    public static readonly string Name = "dontRenewAuthCookie";

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        context.HttpContext.Items.Add(Name, true);
    }
}
