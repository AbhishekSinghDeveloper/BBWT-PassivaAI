using BBWM.Core.Membership.Enums;

namespace BBWM.Core.Membership.DTO;

public class PasswordResetRequestDTO
{
    public string PasswordResetCode { get; set; }
    public PasswordResetRequestReason Reason { get; set; }
}