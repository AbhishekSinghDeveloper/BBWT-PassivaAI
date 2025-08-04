using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Web.Extensions;
using BBWM.FileStorage;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using BBWM.FormIO.Utils;

namespace BBWM.FormIO.Services;

public class FormIOFileService : IFormIOFileService
{
    private readonly IMapper _mapper;
    private readonly IDbContext _dataContext;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly IFileStorageService _fileStorageService;
    private readonly string _userId;

    public FormIOFileService(IMapper mapper,
        IDbContext dataContext,
        IFileStorageProvider fileStorageProvider,
        IHttpContextAccessor httpContextAccessor,
        IFileStorageService fileStorageService)
    {
        _mapper = mapper;
        _dataContext = dataContext;
        _fileStorageProvider = fileStorageProvider;
        _userId = httpContextAccessor.HttpContext.GetUserId();
        _fileStorageService = fileStorageService;
    }

    public async Task<StorageFileData> GetFile(string key, CancellationToken cancellationToken)
    {
        return await _fileStorageProvider.GetFile(key);
    }

    public async Task<FilesUploadingResult> UploadFiles(IFormFile[] files, Dictionary<string, string> additionalData, CancellationToken cancellationToken = default)
    {
        if (files is null) throw new ArgumentNullException(nameof(files));
        if (!files.Any()) throw new AggregateException("There are no files to be uploaded.");

        var result = new FilesUploadingResult();
        var failedFilesCount = 0;
        foreach (var file in files)
        {
            try
            {
                result.SuccessfullyUploadedFiles.Add(await UploadFile(file, additionalData, cancellationToken));
            }
            catch
            {
                failedFilesCount++;
                result.FailedUploadedFileNames.Add(file.FileName);
            }
        }
        if (failedFilesCount > 0)
            result.UploadingStatus = failedFilesCount == files.Length
                ? FilesUploadingStatus.Failed
                : FilesUploadingStatus.PartialSuccess;

        if (result.UploadingStatus != FilesUploadingStatus.Success)
            throw new FilesUploadingException(result);

        return result;
    }

    private async Task<FileDetailsDTO> UploadFile(IFormFile file, Dictionary<string, string> additionalData, CancellationToken cancellationToken = default)
    {
        int? ExtractIntFromAdditionalData(string dataKey)
        {
            int? res = null;
            if (additionalData.TryGetValue(dataKey, out var resStr) && int.TryParse(resStr, out var resVal))
            {
                res = resVal;
            }
            return res;
        }

        DateTimeOffset? ExtractDateFromAdditionalData(string dataKey)
        {
            DateTimeOffset? res = null;
            if (additionalData.TryGetValue(dataKey, out var resStr) && DateTimeOffset.TryParse(resStr, out var resVal))
            {
                res = resVal;
            }
            return res;
        }


        var userId = _userId;
        var operationName = additionalData.ContainsKey("operation_name") ? additionalData["operation_name"] : null;
        var lastModified = ExtractDateFromAdditionalData("last_modified").GetValueOrDefault(DateTimeOffset.UtcNow);
        var extension = Path.GetExtension(file.FileName).Remove(0, 1);
        var key = Guid.NewGuid().ToString();
        var size = file.Length;
        var isImage = IsImage(file);
        string thumbnailKey = null;

        // File saving
        using (var fileStream = file.OpenReadStream())
        {
            // TODO: SixLabors library performs resizing with a bug (making transparent background black)
            /*if (isImage && SupportedMimeType(file))
            {
                var maxSize = ExtractIntFromAdditionalData("max_size").GetValueOrDefault(1500);
                var thumbnailSize = ExtractIntFromAdditionalData("thumbnail_size").GetValueOrDefault(400);
                var degree = ExtractIntFromAdditionalData("degree").GetValueOrDefault(0);
                var scaleX = ExtractIntFromAdditionalData("scaleX").GetValueOrDefault(1);
                var scaleY = ExtractIntFromAdditionalData("scaleY").GetValueOrDefault(1);

                using (var imageStream = ReduceTooLargeImage(fileStream, maxSize, degree, scaleX, scaleY))
                {
                    size = imageStream.Length;
                    await _fileStorageProvider.UploadFile(imageStream, $"{key}.{extension}", cancellationToken);
                }
                using (var thumbnailStream = CreateThumbnailImage(fileStream, thumbnailSize, degree, scaleX, scaleY))
                {
                    try
                    {
                        thumbnailKey = Guid.NewGuid().ToString();
                        await _fileStorageProvider.UploadFile(thumbnailStream, $"{thumbnailKey}.{extension}", cancellationToken);
                    }
                    catch
                    {
                        await _fileStorageProvider.DeleteFile(key, cancellationToken);
                        throw;
                    }
                }
            }
            else
            {
                await _fileStorageProvider.UploadFile(fileStream, $"{key}.{extension}", cancellationToken);
            }*/

            await _fileStorageProvider.UploadFile(fileStream, $"{key}.{extension}", cancellationToken);
        }

        try
        {
            // Creating DB record
            var dbDetails = new FileDetails
            {
                Key = key,
                ThumbnailKey = isImage ? key : null,
                FileName = Path.GetFileNameWithoutExtension(file.FileName),
                Extension = extension,
                SizeBytes = size,
                LastUpdated = lastModified,
                UploadTime = DateTime.UtcNow,
                IsImage = isImage,
                UserId = userId,
                OperationName = operationName
            };

            _dataContext.Set<FileDetails>().Add(dbDetails);
            _dataContext.SaveChanges();

            var _urlFromProvider = await _fileStorageProvider.GetFile($"{key}.{extension}");

            var res = new FileDetailsDTO
            {
                Id = dbDetails.Id,
                FileName = $"{dbDetails.FileName}.{dbDetails.Extension}",
                IsImage = dbDetails.IsImage,
                Key = $"{dbDetails.Key}.{dbDetails.Extension}",
                ThumbnailKey = $"{dbDetails.ThumbnailKey}.{dbDetails.Extension}",
                LastUpdated = dbDetails.LastUpdated,
                Size = dbDetails.SizeBytes,
                ThumbnailUrl = _urlFromProvider.Url,
                UploadTime = dbDetails.UploadTime,
                Url = _urlFromProvider.Url
            };

            return res;
        }
        catch (Exception)
        {
            // If the record creation failed we should remove unbound files
            await _fileStorageProvider.DeleteFile(key, cancellationToken);
            if (!string.IsNullOrEmpty(thumbnailKey))
                await _fileStorageProvider.DeleteFile(thumbnailKey, cancellationToken);

            throw;
        }
    }

    private static bool IsImage(IFormFile file) => file.ContentType.Contains("image");

    public async Task<bool> DeleteMultipleFilesAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        if (keys is null || keys?.Length == 0)
        {
            return true;
        }

        try
        {
            foreach (var key in keys)
            {
                // Delete file from S3 and Db.FileDetails table
                await _fileStorageService.DeleteFile(key, cancellationToken);
            }
        }
        catch
        {
            throw;
        }

        return true;
    }

    public async Task<byte[]> GetAttachmentsZip(int formDataId, CancellationToken ct = default)
    {
        var formData = await _dataContext.Set<FormData>()
                            .Where(x => x.Id == formDataId)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(ct) ?? throw new BusinessException("Form Data doesn't exist.");

        var json = FormDataJsonParser.FromJson(formData.Json);

        var imageUploaderKeys = json?.Data?.ImageUploader;
        var fileAttachments = json?.Data?.FileAttachments?.ToArray();

        List<ZipFileInfo> filesToZip = new List<ZipFileInfo>();

        if (imageUploaderKeys is not null)
        {
            foreach (var imageUploader in imageUploaderKeys)
            {
                // need more properties the imageuploader component
                filesToZip.Add(await GetZipFileInfo(imageUploader.Value.Key, imageUploader.Value.FileName, ct));
            }
        }

        if (fileAttachments?.Length > 0)
        {
            foreach (var fileAttachment in fileAttachments)
            {
                filesToZip.Add(await GetZipFileInfo(fileAttachment.Key, fileAttachment.OriginalName, ct));
            }
        }

        return await GetZipArchive(filesToZip, ct);

    }

    private async Task<ZipFileInfo> GetZipFileInfo(string key, string originalName, CancellationToken ct)
    {
        string[] nameExtension = originalName.Split(".");
        string _name = nameExtension[0] ?? "attachment";
        string _extension = nameExtension[1] ?? "jpg";

        // TODO: FIX THIS
        byte[] fileStream =  await _fileStorageService.DownloadFile(key, ct);

        return new ZipFileInfo
        {
            Name = _name,
            Extension = _extension,
            FileStream = fileStream
        };
    }

    private async Task<byte[]> GetZipArchive(List<ZipFileInfo> filesToZip, CancellationToken ct)
    {
        // create a zip archive in memory
        using MemoryStream memoryStream = new();

        using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in filesToZip)
            {
                using var stream = new MemoryStream(file.FileStream);

                var entry = zipArchive.CreateEntry($"{file.Name}.{file.Extension}", CompressionLevel.Optimal);

                using var entryStream = entry.Open();

                await stream.CopyToAsync(entryStream, ct);
            }
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream.ToArray();
    }
}
