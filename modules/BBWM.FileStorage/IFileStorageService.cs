using Microsoft.AspNetCore.Http;

namespace BBWM.FileStorage;

public interface IFileStorageService
{
    Task<FileDetailsDTO> Get(int id);
    Task<ICollection<FileDetailsDTO>> GetAllFiles(string operationName = null, CancellationToken cancellationToken = default);
    Task<ICollection<FileDetailsDTO>> GetAllFiles(CancellationToken cancellationToken);
    Task<ICollection<FileDetailsDTO>> GetAllImages(string operationName = null, CancellationToken cancellationToken = default);
    Task<ICollection<FileDetailsDTO>> GetAllImages(CancellationToken cancellationToken);

    Task<FilesUploadingResult> UploadFiles(IFormFile[] files, Dictionary<string, string> additionalData, CancellationToken cancellationToken = default);
    Task CompleteUsersFilesUploadingOperation(string userId, string operationName, CancellationToken cancellationToken = default);
    Task<bool> DeleteFile(string fileDetailsKey, CancellationToken cancellationToken = default);
    Task<bool> DeleteFile(int fileDetailsId, CancellationToken cancellationToken = default);
    Task<byte[]> DownloadFile(string key, CancellationToken cancellationToken = default);
}
