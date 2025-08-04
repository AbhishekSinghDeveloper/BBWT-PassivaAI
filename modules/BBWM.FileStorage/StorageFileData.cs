namespace BBWM.FileStorage;

public class StorageFileData
{
    public string Key { get; set; }
    public string Url { get; set; }
    public long Size { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public bool IsImage { get; set; }
}
