namespace BBWM.SystemData.DTO;

public class SystemSummaryDTO
{
    /// <summary>
    /// .Net Core Environment name (Development/Production...)
    /// </summary>
    public string ServerEnvironment { get; set; }

    /// <summary>
    /// Server machine name
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// Local server IP
    /// </summary>
    public string ServerIp { get; set; }

    /// <summary>
    /// Operating System
    /// </summary>
    public string OperatingSystem { get; set; }

    /// <summary>
    /// Remote client IP
    /// </summary>
    public string ClientIp { get; set; }

    /// <summary>
    /// Login of authorized user
    /// </summary>
    public string UserName { get; set; }
}
