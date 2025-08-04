namespace BBWM.FormIO.DTO;

public class ZipFileInfo
{
    public string Name { get; set; } = null!;
    public string Extension { get; set; } = null!;
    public string? OriginalName { get; set; }
    public byte[] FileStream { get; set; } = null!;
}