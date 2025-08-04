using Azure.Storage.Blobs;

namespace BBWM.Azure;

public interface IAzureContainerClientFactory
{
    Task<BlobContainerClient> CreateContainerAsync(CancellationToken ct = default);
}
