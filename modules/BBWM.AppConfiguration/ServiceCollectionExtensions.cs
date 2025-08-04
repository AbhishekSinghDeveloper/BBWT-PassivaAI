using Microsoft.Extensions.DependencyInjection;

namespace BBWM.AppConfiguration;

public static class ServiceCollectionExtensions
{
    public static void SetFakeAppConfigurationService(this IServiceCollection services)
    {
        services.AddScoped<IAppConfigurationService, FakeAppConfigurationService>();
    }
}
