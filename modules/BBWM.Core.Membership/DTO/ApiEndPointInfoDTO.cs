namespace BBWM.Core.Membership.DTO;

/// <summary>
/// Information about API endpoint.
/// </summary>
public class ApiEndPointInfoDTO
{
    /// <summary>
    /// HTTP Method.
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// Action path.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Allowed roles.
    /// </summary>
    public List<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// Allowed permissions.
    /// </summary>
    public List<string> Permissions { get; set; } = new List<string>();
}
