using BBWM.Core.Data;

using Microsoft.AspNetCore.Identity;

namespace BBWM.Metadata;

public abstract class MetadataModelBase : IAuditableEntity
{
    public int Id { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public DateTimeOffset LastUpdated { get; set; }

    public string UserId { get; set; }

    public bool IsLocked { get; set; }

    public string LockedByUserId { get; set; }
}

public abstract class MetadataModel<TUser> : MetadataModelBase
    where TUser : IdentityUser
{
    public TUser User { get; set; }

    public TUser LockedByUser { get; set; }
}
