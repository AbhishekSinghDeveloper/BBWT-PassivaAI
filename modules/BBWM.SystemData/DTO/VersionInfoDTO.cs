namespace BBWM.SystemData.DTO;

public class VersionInfoDTO
{
    public string FullProductVersion { get; set; }
    public string ProductVersion { get; set; }
    public string Pipeline { get; set; }
    public string CommitHash { get; set; }
    public string ProjectName { get; set; }
    public string ProjectID { get; set; }
}
