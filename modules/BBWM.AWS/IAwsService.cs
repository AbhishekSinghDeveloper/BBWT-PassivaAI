using Amazon;

using BBWM.FileStorage;

namespace BBWM.AWS;

public interface IAwsService
{
    ICollection<RegionEndpoint> GetRegions();
    Task<AwsCheckPermissionsResult> CheckPermissions();
    string GeneratePreSignedURL(string key);
    Task<bool> DeleteFile(string key);
    Task<StorageFileData> UploadFile(Stream stream, string key, CancellationToken cancellationToken);
    Task<ICollection<StorageFileData>> GetAllFiles(CancellationToken cancellationToken);
    Task<ICollection<StorageFileData>> GetAllImages(CancellationToken cancellationToken);
    Task<StorageFileData> GetFile(string key, CancellationToken cancellationToken);
    Task<byte[]> DownloadFile(string key, CancellationToken cancellationToken);
}
