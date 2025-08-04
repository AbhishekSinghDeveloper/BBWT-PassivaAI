using AutoMapper;
using BBWM.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace BBWM.FileStorage;

public class FileStorageService : IFileStorageService
{
    private readonly IMapper _mapper;
    private readonly IDbContext _dataContext;
    private readonly IFileStorageProvider _fileStorageProvider;


    public FileStorageService(
        IMapper mapper,
        IDbContext dataContext,
        IFileStorageProvider fileStorageProvider)
    {
        _mapper = mapper;
        _dataContext = dataContext;
        _fileStorageProvider = fileStorageProvider;
    }


    public async Task<FileDetailsDTO> Get(int id) =>
        _mapper.Map<FileDetailsDTO>(await _dataContext.Set<FileDetails>().FindAsync(id));

    public async Task<ICollection<FileDetailsDTO>> GetAllFiles(string operationName = null, CancellationToken cancellationToken = default)
    {
        var fileDetails = await _dataContext.Set<FileDetails>()
            .Where(x => string.IsNullOrEmpty(operationName) || x.OperationName == operationName)
            .ToListAsync(cancellationToken);
        return fileDetails.Select(x => _mapper.Map<FileDetailsDTO>(x)).ToList();
    }

    public async Task<ICollection<FileDetailsDTO>> GetAllFiles(CancellationToken cancellationToken) =>
        await GetAllFiles(null, cancellationToken);

    public async Task<ICollection<FileDetailsDTO>> GetAllImages(string operationName = null, CancellationToken cancellationToken = default)
    {
        var fileDetails = await _dataContext.Set<FileDetails>()
            .Where(x => x.IsImage && (string.IsNullOrEmpty(operationName) || x.OperationName == operationName))
            .OrderByDescending(x => x.LastUpdated)
            .ToListAsync(cancellationToken);
        return fileDetails.Select(x => _mapper.Map<FileDetailsDTO>(x)).ToList();
    }

    public async Task<ICollection<FileDetailsDTO>> GetAllImages(CancellationToken cancellationToken) =>
        await GetAllImages(null, cancellationToken);

    public async Task<FilesUploadingResult> UploadFiles(IFormFile[] files, Dictionary<string, string> additionalData, CancellationToken cancellationToken = default)
    {
        if (files is null) throw new ArgumentNullException(nameof(files));
        if (!files.Any()) throw new AggregateException("There are no files to be uploaded.");

        // Removing old files and DB records based on existing saved operation for the user
        if (additionalData.ContainsKey("user_id") && additionalData.ContainsKey("operation_name"))
        {
            var fileDetails = GetFileDetailsByUserAndOperationName(
                additionalData["user_id"], additionalData["operation_name"]);
            foreach (var fileDetailsItem in fileDetails)
            {
                await DeleteFileDetails(fileDetailsItem, cancellationToken);
            }
        }

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

    public async Task<bool> DeleteFile(string fileDetailsKey, CancellationToken cancellationToken = default)
    {
        var pureKey = Path.GetFileNameWithoutExtension(fileDetailsKey);
        var fileDetails = await GetFileDetails(pureKey, cancellationToken);

        return fileDetails is not null && await DeleteFileDetails(fileDetails, cancellationToken);
    }

    public async Task<bool> DeleteFile(int fileDetailsId, CancellationToken cancellationToken = default)
    {
        var fileDetails = await GetFileDetails(fileDetailsId);

        return fileDetails is not null && await DeleteFileDetails(fileDetails, cancellationToken);
    }

    public async Task CompleteUsersFilesUploadingOperation(string userId, string operationName,
        CancellationToken cancellationToken = default)
    {
        var fileDetailsList = GetFileDetailsByUserAndOperationName(userId, operationName);

        foreach (var fileDetailsItem in fileDetailsList)
        {
            fileDetailsItem.UserId = null;
            fileDetailsItem.OperationName = null;
        }

        await _dataContext.SaveChangesAsync(cancellationToken);
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


        var key = Guid.NewGuid().ToString();
        var userId = additionalData.ContainsKey("user_id") ? additionalData["user_id"] : null;
        var operationName = additionalData.ContainsKey("operation_name") ? additionalData["operation_name"] : null;
        var lastModified = ExtractDateFromAdditionalData("last_modified").GetValueOrDefault(DateTimeOffset.UtcNow);
        var extension = Path.GetExtension(file.FileName).Remove(0, 1);
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

            var res = _mapper.Map<FileDetailsDTO>(dbDetails);

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

    private static Stream ReduceTooLargeImage(Stream input, int maxSize, int degree, int scaleX, int scaleY) =>
        ResizeImage(input, maxSize, degree, scaleX, scaleY, 100, false);

    private static Stream CreateThumbnailImage(Stream input, int thumbnailSize, int degree, int scaleX, int scaleY) =>
        ResizeImage(input, thumbnailSize, degree, scaleX, scaleY);

    private static Stream ResizeImage(Stream input, int sizeToFit, int degree, int scaleX, int scaleY, int quality = 75, bool directly = true)
    {
        Stream output = new MemoryStream();

        input.Seek(0, SeekOrigin.Begin);
        using (var image = Image.Load<Rgba32>(input))
        {
            if (directly)
                Resize(image);
            else
            {
                var ratio = image.Height / (float)image.Width;
                if (ratio < 1)
                    ratio = 1 / ratio;

                if (ratio > 2) // too long image
                    Resize(image, ResizeMode.Min);
                else
                    Resize(image);
            }

            if (degree > 0)
            {
                image.Mutate(x => x.Rotate(degree));
            }

            if (scaleX * scaleY < 0)
            {
                if (scaleX == -1)
                    image.Mutate(x => x.Flip(FlipMode.Horizontal));
                else
                    image.Mutate(x => x.Flip(FlipMode.Vertical));
            }

            var encoder = new JpegEncoder();

            image.Metadata.ExifProfile = image.Metadata.ExifProfile ?? new ExifProfile();

            image.Metadata.ExifProfile.SetValue(ExifTag.Orientation, (ushort)1);

            image.Save(output, encoder);
        }

        return output;

        void Resize(Image<Rgba32> img, ResizeMode mode = ResizeMode.Max)
        {
            int sideSize;
            switch (mode)
            {
                case ResizeMode.Min:
                    sideSize = Math.Min(img.Height, img.Width);
                    break;
                default:
                    sideSize = Math.Max(img.Height, img.Width);
                    break;
            }
            if (sideSize < sizeToFit) return;

            var options = new ResizeOptions
            {
                Size = new Size(sizeToFit, sizeToFit),
                Mode = mode
            };

            img.Mutate(x => x.Resize(options));
        }
    }


    private static bool IsImage(IFormFile file) => file.ContentType.Contains("image");

    private async Task<bool> DeleteFileDetails(FileDetails fileDetails,
        CancellationToken cancellationToken)
    {
        var filesResult = await _fileStorageProvider.DeleteFile($"{fileDetails.Key}.{fileDetails.Extension}", cancellationToken);
        if (fileDetails.IsImage)
            filesResult &= await _fileStorageProvider.DeleteFile($"{fileDetails.ThumbnailKey}.{fileDetails.Extension}", cancellationToken);

        _dataContext.Set<FileDetails>().Remove(fileDetails);
        var removedRecordsCount = await _dataContext.SaveChangesAsync(cancellationToken);

        return removedRecordsCount > 0 && filesResult;
    }

    private async Task<FileDetails> GetFileDetails(string fileDetailsKey, CancellationToken cancellationToken) =>
        await _dataContext.Set<FileDetails>().FirstOrDefaultAsync(item => item.Key == fileDetailsKey, cancellationToken);

    private async Task<FileDetails> GetFileDetails(int fileDetailsId) =>
        await _dataContext.Set<FileDetails>().FindAsync(fileDetailsId);

    private FileDetails[] GetFileDetailsByUserAndOperationName(string userId, string operationName) =>
        _dataContext.Set<FileDetails>()
            .Where(item => item.UserId == userId && item.OperationName == operationName)
            .ToArray();

    public async Task<byte[]> DownloadFile(string key, CancellationToken cancellationToken = default)
    {
        return await _fileStorageProvider.DownloadFile(key, cancellationToken);
    }

}
