using System.Runtime.Serialization;

namespace BBWM.Core.Membership.Enums;

/// <summary>
/// Defines users status.
/// </summary>
public enum AccountStatus
{
    /// <summary>
    /// An invitation email has been sent but the user has not taken required action yet.
    /// </summary>
    [EnumMember(Value = "Invited")] Invited = 1,

    /// <summary>
    /// A system administrator must approve the account before it can be used.
    /// </summary>
    [EnumMember(Value = "Unapproved")] Unapproved = 2,

    /// <summary>
    /// The account has been approved by an administrator approved but the email address has not.
    /// </summary>
    [EnumMember(Value = "Unverified")] Unverified = 3,

    /// <summary>
    /// The account setup process is complete and it is ready to use.
    /// </summary>
    [EnumMember(Value = "Active")] Active = 4,

    /// <summary>
    /// The account has been temporarily suspended (by an administrator) and cannot be used.
    /// </summary>
    [EnumMember(Value = "Suspended")] Suspended = 5,

    /// <summary>
    /// The account has been flagged as deleted, and cannot be used.
    /// </summary>
    [EnumMember(Value = "Deleted")] Deleted = 6
}
