using BBWM.Core.Exceptions;
using BBWM.FileStorage;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Azure;

public static class ServiceCollectionExtensions
{
    public static void ConfigureAzure(this IServiceCollection services, IConfiguration configuration, string azureSectionName = "AzureSettings")
    {
        services.AddScoped<IFileStorageProvider, AzureStorageProvider>();

        var azureSection = configuration.GetSection(azureSectionName);
        if (string.IsNullOrWhiteSpace(azureSection.GetValue<string>("ConnectionString")))
            throw new EmptyConfigurationSectionException($"{azureSectionName}.ConnectionString");
        if (string.IsNullOrWhiteSpace(azureSection.GetValue<string>("ContainerName")))
            throw new EmptyConfigurationSectionException($"{azureSectionName}.ContainerName");

        services.Configure<AzureSettings>(azureSection);

        services.AddScoped<IAzureContainerClientFactory, AzureContainerClientFactory>();
    }
}
