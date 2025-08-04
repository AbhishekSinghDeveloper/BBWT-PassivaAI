using Microsoft.Extensions.DependencyInjection;

namespace BBWM.FileStorage.DiskSpace;

public static class ServiceCollectionExtensions
{
    public static void ConfigureDiskSpaceStorageProvider(this IServiceCollection services) =>
        services.AddScoped<IFileStorageProvider, DiskSpaceStorageProvider>();
}
