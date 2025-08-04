using Autofac;

using BBWM.AWS.EventBridge.Interfaces;
using BBWM.Core.Autofac;
using BBWM.Core.Exceptions;
using BBWM.Core.ModuleLinker;

using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.AWS.EventBridge;

public class ConfigureDependenciesLinkage : IDependenciesModuleLinkage, IServicesModuleLinkage
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        AssertSettings(configuration);

        services.Configure<AwsEventBridgeSettings>(
            configuration.GetSection(AwsEventBridgeSettings.CONFIG_SECTION));
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IAwsEventBridgeClientFactory, AwsEventBridgeClientFactory>();

        builder.RegisterType<TrackingJobContext>();

        EventBridgeModuleLinker
            .GetEventBridgeJobImplementors()
            .ForEach(t =>
            {
                builder.RegisterType(t.JobType).InstancePerOwned<TrackingJobContext>();
                builder.RegisterType(t.JobMetadata).SingleInstance().As(typeof(IEventBridgeJobMetadata<>).MakeGenericType(t.JobType));
            });

        builder
            .RegisterType<NoOpJobErrorHandler>()
            .IfNotRegistered(typeof(IEventBridgeJobErrorHandler))
            .As<IEventBridgeJobErrorHandler>().InstancePerOwned<TrackingJobContext>();
    }

    private static void AssertSettings(IConfiguration configuration)
    {
        var settings = configuration.GetSection(AwsEventBridgeSettings.CONFIG_SECTION)?.Get<AwsEventBridgeSettings>();

        bool missingSettings = new[] {
                    settings?.APIKey,
                    settings?.TargetRoleArn,
                    settings?.ApiConnectionName,
                    settings?.ApiDestinationName
            }.Any(string.IsNullOrEmpty);

        if (missingSettings)
            throw new ConflictException("Some AWS EB required settings are missing!");
    }
}
