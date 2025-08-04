namespace BBWM.Core.Membership.DTO;

public class LoginDTO
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string CaptchaResponse { get; set; }
    public string Fingerprint { get; set; }
    public string Browser { get; set; }
    public string TwoFactorCode { get; set; }
    public string TwoFactorRecoveryCode { get; set; }
    public string RealFirstName { get; set; }
    public string RealLastName { get; set; }
    public string RealEmail { get; set; }
    /// <summary>
    /// Password required on password renewal
    /// </summary>
    public string NewPassword { get; set; }
}
