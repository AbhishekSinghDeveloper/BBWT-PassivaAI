namespace BBWM.FileStorage;

public class FileDetailsDTO
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string ThumbnailKey { get; set; }

    public string Url { get; set; }
    public string ThumbnailUrl { get; set; }

    public bool IsImage { get; set; }

    public string FileName { get; set; }

    public long Size { get; set; }
    public DateTimeOffset UploadTime { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}
