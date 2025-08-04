using BBWM.FileStorage;

namespace BBWM.AWS;

public class AwsFileStorageProvider : IFileStorageProvider
{
    private readonly IAwsService _awsService;

    public AwsFileStorageProvider(IAwsService awsService)
    {
        _awsService = awsService;
    }

    public Task<StorageFileData> UploadFile(Stream stream, string key, CancellationToken cancellationToken)
    {
        return _awsService.UploadFile(stream, key, cancellationToken);
    }

    public Task<StorageFileData> GetFile(string key, CancellationToken cancellationToken)
    {
        return _awsService.GetFile(key, cancellationToken);
    }

    public Task<bool> DeleteFile(string key, CancellationToken cancellationToken)
    {
        return _awsService.DeleteFile(key);
    }

    public Task<byte[]> DownloadFile(string key, CancellationToken cancellationToken)
    {
        return _awsService.DownloadFile(key, cancellationToken);
    }
}
