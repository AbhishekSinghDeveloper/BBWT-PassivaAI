using Azure.Storage.Blobs.Models;

using BBWM.FileStorage;

namespace BBWM.Azure;

public class AzureStorageProvider : IFileStorageProvider
{
    private readonly IAzureContainerClientFactory _containerFactory;

    public AzureStorageProvider(IAzureContainerClientFactory containerFactory) => _containerFactory = containerFactory;

    public async Task<StorageFileData> UploadFile(Stream stream, string key, CancellationToken cancellationToken = default)
    {
        var container = await _containerFactory.CreateContainerAsync(cancellationToken);
        var blob = container.GetBlobClient(key);
        await blob.UploadAsync(stream, cancellationToken);

        return new StorageFileData
        {
            Key = key,
            LastModifiedDate = DateTime.Now,
            Url = blob.Uri.ToString(),
            Size = stream.Length
        };
    }

    public async Task<StorageFileData> GetFile(string key, CancellationToken cancellationToken = default)
    {
        var container = await _containerFactory.CreateContainerAsync(cancellationToken);
        var blob = container.GetBlobClient(key);

        var properties = (await blob.GetPropertiesAsync(cancellationToken: cancellationToken))?.Value;

        return new StorageFileData
        {
            Key = key,
            LastModifiedDate = properties?.LastModified.DateTime ?? DateTime.Now,
            Url = blob.Uri.ToString(),
            Size = properties?.ContentLength ?? 0
        };
    }

    public async Task<bool> DeleteFile(string key, CancellationToken cancellationToken = default)
    {
        var container = await _containerFactory.CreateContainerAsync(cancellationToken);
        var blob = container.GetBlobClient(key);

        return await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
    }

    public async Task<byte[]> DownloadFile(string key, CancellationToken cancellationToken = default)
    {
        var container = await _containerFactory.CreateContainerAsync(cancellationToken);
        var blob = container.GetBlobClient(key);

        using var memoryStream = new MemoryStream();
        await blob.DownloadToAsync(memoryStream, cancellationToken);

        return memoryStream.ToArray();
    }

}
