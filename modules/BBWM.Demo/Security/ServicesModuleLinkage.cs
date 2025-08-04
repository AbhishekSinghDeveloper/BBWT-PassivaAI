using BBWM.Core.ModuleLinker;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Demo.Security;

public class ServicesModuleLinkage : IServicesModuleLinkage
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Below, we add group-based security over role-based security.
        // If you required state-based security for your application you would add that below too
        // State-based security - Rarely, you can also have other resource-based requirements that aren’t
        // specifically about group membership e.g. “if entity Y *currently* passes a test with respect to the
        // calling user Z”.
        services.AddAuthorization(options =>
            {
                // Group policies
                options.AddPolicy(Policies.BelongsToGroup, policy => policy.AddRequirements(new AccessibleToGroupRequirement()));
            })
        .AddSingleton<IAuthorizationHandler, AcessibleToGroupForListAuthorizationHandler>()
        .AddSingleton<IAuthorizationHandler, AccessibleToGroupByIdAuthorizationHandler>();
    }
}
