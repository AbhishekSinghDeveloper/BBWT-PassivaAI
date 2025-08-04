using BBWM.Core.Data;

namespace BBWM.FileStorage;

/// <summary>
/// Represents a record about an uploaded file.
/// </summary>
public class FileDetails : IEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Represents a name of corresponding file on a disk space without extension.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Represents a name of corresponding thumbnail on a disk space without extension if the file is image.
    /// </summary>
    public string ThumbnailKey { get; set; }

    /// <summary>
    /// Actual filename.
    /// </summary>
    public string FileName { get; set; }

    public string Extension { get; set; }

    public long SizeBytes { get; set; }

    public DateTimeOffset UploadTime { get; set; }

    public DateTimeOffset LastUpdated { get; set; }

    public bool IsImage { get; set; }

    /// <summary>
    /// Defines the user who has performed file uploading.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Defines the operation name responsible for the file uploading.
    /// </summary>
    public string OperationName { get; set; }
}
