
using Microsoft.AspNetCore.Http;


namespace BBWM.Core.Web.Middlewares
{
    public class AddSecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public AddSecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("Cross-Origin-Resource-Policy", "same-origin");
            context.Response.Headers.Add("Cross-Origin-Opener-Policy", "same-origin");
            context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", "none");
            await _next(context);
        }
    }
}

