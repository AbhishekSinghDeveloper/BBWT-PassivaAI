namespace BBWM.Core.Membership.DTO;

public class SignUpResultDTO
{
    public bool AdminApprovalRequired { get; set; }

    public bool ConfirmationSent { get; set; }

    public int PwnedResult { get; set; }
}
