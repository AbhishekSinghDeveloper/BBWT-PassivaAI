using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using BBWM.Core.Exceptions;

using Microsoft.Extensions.Options;

namespace BBWM.Azure;

public class AzureContainerClientFactory : IAzureContainerClientFactory
{
    private readonly AzureSettings _azureSettings;

    public AzureContainerClientFactory(IOptionsSnapshot<AzureSettings> azureSettings)
    {
        AssertAzureSettings(azureSettings?.Value);

        _azureSettings = azureSettings.Value;
    }

    public async Task<BlobContainerClient> CreateContainerAsync(CancellationToken ct = default)
    {
        var container = new BlobContainerClient(_azureSettings.ConnectionString, _azureSettings.ContainerName);
        await container.SetAccessPolicyAsync(PublicAccessType.BlobContainer, cancellationToken: ct);

        return container;
    }

    private static void AssertAzureSettings(AzureSettings settings)
    {
        var anyWrongSetting =
            new[]
            {
                    settings?.ConnectionString,
                    settings?.ContainerName,
            }
            .Any(string.IsNullOrEmpty);

        if (anyWrongSetting)
            throw new ConflictException("Check out Azure settings!");
    }
}
