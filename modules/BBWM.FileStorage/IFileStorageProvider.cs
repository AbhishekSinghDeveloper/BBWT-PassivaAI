namespace BBWM.FileStorage;

public interface IFileStorageProvider
{
    Task<StorageFileData> UploadFile(Stream stream, string key, CancellationToken cancellationToken = default);
    Task<StorageFileData> GetFile(string key, CancellationToken cancellationToken = default);
    Task<bool> DeleteFile(string key, CancellationToken cancellationToken = default);
    Task<byte[]> DownloadFile(string key, CancellationToken cancellationToken = default);
}
