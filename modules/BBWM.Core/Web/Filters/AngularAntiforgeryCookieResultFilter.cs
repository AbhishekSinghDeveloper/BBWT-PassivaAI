using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BBWM.Core.Web.Filters;

public class AngularAntiforgeryCookieResultFilter : ResultFilterAttribute
{
    private readonly IAntiforgery _antiForgery;

    public AngularAntiforgeryCookieResultFilter(IAntiforgery antiForgery)
    {
        _antiForgery = antiForgery;
    }

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (!(context.Result is ViewResult)) return;

        var tokens = _antiForgery.GetAndStoreTokens(context.HttpContext);
        context.HttpContext.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = true
        });
    }
}
