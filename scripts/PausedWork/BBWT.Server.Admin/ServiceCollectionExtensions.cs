using AspNetCoreRateLimit;

using Autofac;

using BBWM.Core.Autofac;
using BBWM.Core.Membership.Model;
using BBWM.Maintenance;
using BBWM.Metadata;
using BBWM.SystemSettings;

using BBWT.Data.Model;

namespace BBWT.Server.Admin;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSpecificServices(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        // AspNetCoreRateLimit inject counter and rules distributed cache stores
        services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();

        return services;
    }

    public static IServiceCollection AddFilters(this IServiceCollection services)
    {
        return services;
    }

    public static void RegisterBbwtSettings(this IApplicationBuilder app)
    {
        // System settings
        app.RegisterSystemSettings();

        // Maintenance
        app.RegisterMaintenanceSettings();
    }

    public static ContainerBuilder AddBBWTServices(this ContainerBuilder builder)
    {
        builder.RegisterLoggingInterceptor();

        // metadata
        builder.RegisterMetadataServices<Metadata, User>();

        return builder;
    }
}
