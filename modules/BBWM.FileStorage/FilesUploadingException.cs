using BBWM.Core.Exceptions;

namespace BBWM.FileStorage;

public sealed class FilesUploadingException : DataException
{
    public FilesUploadingException(FilesUploadingResult uploadingResult)
        : base("Files uploading error occurred with next files: " +
              $"{string.Join(' ', uploadingResult.FailedUploadedFileNames ?? Array.Empty<string>())}") =>
        UploadingResult = uploadingResult;


    public FilesUploadingResult UploadingResult { get; }
}
