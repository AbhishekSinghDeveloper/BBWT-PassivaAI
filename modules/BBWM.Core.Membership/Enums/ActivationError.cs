using System.Runtime.Serialization;

namespace BBWM.Core.Membership.Enums;

public enum ActivationError
{
    [EnumMember(Value = "0")] ActivationCompleted,
    [EnumMember(Value = "1")] InvitationNotFoundForUser,
    [EnumMember(Value = "2")] ActivationCodeInvalid
}