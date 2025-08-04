using BBWM.Core.DTO;

namespace BBWM.Core.Membership.DTO;

/// <summary>
/// Data transfer object for groups
/// </summary>
public class GroupDTO : IDTO
{
    /// <summary>
    /// Entity identity field
    /// </summary>
    public int Id { get; set; }
    public string Name { get; set; }
}
