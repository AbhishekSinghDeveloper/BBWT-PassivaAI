using Autofac;
using AutoMapper;
using BBWM.Core.AppEnvironment;
using BBWM.Core.Autofac;
using BBWM.Core.Data;
using BBWM.Core.ModelHashing;
using BBWM.Core.ModuleLinker;
using BBWM.Core.Services;
using BBWM.Core.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core;

public class CoreModuleLinkage : IConfigureModuleLinkage, IServicesModuleLinkage, IDependenciesModuleLinkage
{
    public void ConfigureModule(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var mapper = serviceScope.ServiceProvider.GetService<IMapper>();
        var context = serviceScope.ServiceProvider.GetService<IDbContext>();
        var modelHashingService = serviceScope.ServiceProvider.GetService<IModelHashingService>();
        modelHashingService.Register(mapper, context);
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Background tasks services
        services.AddHostedService<QueuedHostedService>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IDataService, DataService>();
        builder.RegisterService<IDbServices, DbServices>();
        builder.RegisterService<IAppEnvironmentService, AppEnvironmentService>();
    }
}
