namespace BBWM.FileStorage;

public enum FilesUploadingStatus
{
    Failed,
    PartialSuccess,
    Success
}

public class FilesUploadingResult
{
    public FilesUploadingResult()
    {
        SuccessfullyUploadedFiles = new List<FileDetailsDTO>();
        FailedUploadedFileNames = new List<string>();
        UploadingStatus = FilesUploadingStatus.Success;
    }


    public IList<FileDetailsDTO> SuccessfullyUploadedFiles { get; set; }

    public IList<string> FailedUploadedFileNames { get; set; }

    public FilesUploadingStatus UploadingStatus { get; set; }
}
