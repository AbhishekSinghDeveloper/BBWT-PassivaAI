using Microsoft.AspNetCore.Builder;

namespace BBWM.Core.Web.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpToHttpsRedirectMiddleware(this IApplicationBuilder builder) =>
            builder.UseMiddleware<HttpToHttpsRedirectMiddleware>();

        public static IApplicationBuilder UseAddUserToLogsMiddleware(this IApplicationBuilder builder) =>
            builder
                .UseMiddleware<AddUsernameToLogsMiddleware>()
                .UseMiddleware<AddUserImpersonationToLogsMiddleware>()
                .UseMiddleware<AddUserIpToLogsMiddleware>();

        public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder builder) =>
            builder.UseMiddleware<ErrorHandlingMiddleware>();

        public static IApplicationBuilder UseSecurityHeadersMiddleware(this IApplicationBuilder builder) =>
            builder.UseMiddleware<AddSecurityHeadersMiddleware>();
    }
}


