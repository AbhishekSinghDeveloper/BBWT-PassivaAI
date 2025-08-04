using BBWM.Core.Membership.Utils;

using BBWT.Services;

namespace BBWT.Server.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection ConfigureSecurity(this IServiceCollection services)
    {
        // Below, we add group-based and permission-based security over role-based security.
        // If you required state-based security for your application you would add that below too
        // State-based security - Rarely, you can also have other resource-based requirements that aren’t
        // specifically about group membership e.g. “if entity Y *currently* passes a test with respect to the
        // calling user Z”.
        // (The Demo module has an example of group-based security (see Demo's services.AddAuthorization(...))

        services.AddAuthorization(options =>
        {
            foreach (var permission in PermissionsExtractor.GetPermissionNamesOfClass(typeof(Permissions)))
            {
                options.AddPolicy(permission, builder => builder.RequireClaim(permission));
            }
        });

        return services;
    }
}
