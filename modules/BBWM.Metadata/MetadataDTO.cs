namespace BBWM.Metadata;

public class MetadataDTO
{
    public int Id { get; set; }
    /// <summary>
    /// Name
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Age
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Date of record creation
    /// </summary>
    public DateTimeOffset CreatedOn { get; set; }

    /// <summary>
    /// Date of record creation
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; }

    /// <summary>
    /// Last Updated User Id
    /// </summary>
    public string UserId { get; set; }
    public bool IsLocked { get; set; }

    /// <summary>
    /// Last Updated User Id
    /// </summary>
    public string LockedByUserId { get; set; }
    public string LockedByUserFullName { get; set; }
}
