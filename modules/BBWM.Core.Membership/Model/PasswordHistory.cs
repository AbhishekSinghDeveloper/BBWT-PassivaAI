using BBWM.Core.Data;

namespace BBWM.Core.Membership.Model;

/// <summary>
/// Password History
/// </summary>
public class PasswordHistory : IEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Linked user
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Date of creation of record
    /// </summary>
    public DateTimeOffset CreateDate { get; set; }
}
