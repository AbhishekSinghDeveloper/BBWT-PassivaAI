using Autofac;

using BBWM.Core.Autofac;
using BBWM.Core.Membership;

using Microsoft.Extensions.DependencyInjection;

namespace BBWT.InitialData;

public static class ServiceCollectionExtensions
{
    public static void RegisterInitialDataServices(this ContainerBuilder builder)
    {
        builder.RegisterService<IRouteRolesModule, RouteRoles>();
        builder.RegisterService<IRouteRolesModule, RouteRolesCore>();
    }

    public static void EnsureInitialData(this IServiceScope serviceScope, bool includingOnceSeededData)
    {
        var databaseInitializerService = serviceScope.ServiceProvider.GetService<IDatabaseInitializerService>();
        databaseInitializerService.EnsureInitialData(includingOnceSeededData);

        var projectDataInitializerService = serviceScope.ServiceProvider.GetService<IProjectDataInitializerService>();
        projectDataInitializerService.EnsureInitialData(includingOnceSeededData);

        var menuInitializerService = serviceScope.ServiceProvider.GetService<IMenuInitializerService>();
        menuInitializerService.EnsureInitialData().Wait();
    }
}
