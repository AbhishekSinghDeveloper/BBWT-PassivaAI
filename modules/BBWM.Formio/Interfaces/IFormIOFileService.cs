using Microsoft.AspNetCore.Http;

using BBWM.FileStorage;

namespace BBWM.FormIO.Interfaces;

public interface IFormIOFileService
{
    Task<FilesUploadingResult> UploadFiles(IFormFile[] files, Dictionary<string, string> additionalData, CancellationToken cancellationToken = default);
    Task<bool> DeleteMultipleFilesAsync(string[] keys, CancellationToken cancellationToken = default);
    Task<StorageFileData> GetFile(string key, CancellationToken cancellationToken);
    Task<byte[]> GetAttachmentsZip(int formDataId, CancellationToken ct = default);
}
