using Autofac;

using BBWM.Core.Autofac;
using BBWM.Core.Membership.Utils;
using BBWM.Core.ModuleLinker;
using BBWM.Core.Services;
using BBWM.Demo.Northwind.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Demo.ModuleLinkage;

public class ServicesModuleLinkage : IServicesModuleLinkage, IDependenciesModuleLinkage
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization(options =>
        {
            // Permissions policies
            var permissionsNames = PermissionsExtractor.GetPermissionNamesOfClass(typeof(Permissions));
            foreach (var permissionName in permissionsNames)
            {
                options.AddPolicy(permissionName, policyBuilder => policyBuilder.RequireClaim(permissionName));
            }
        });
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IDataService<IDemoDataContext>, DataService<IDemoDataContext>>();

        // Data generation through HubB
        builder.RegisterService<IRandomDataService, RandomDataService>(ServiceLifetime.Singleton);
    }
}
