namespace BBWM.Core.Membership.DTO;

public class AuthResultDTO
{
    public string UserId { get; set; }
    public bool AuthenticatorEnabled { get; set; }
    public bool U2FEnabled { get; set; }
    public bool LockoutUserEnabled { get; set; }
    public bool LockoutIpEnabled { get; set; }
    public int? LockoutTimeoutInSeconds { get; set; }
    public bool IsSystemTester { get; set; }
    public UserDTO LoggedUser { get; set; }
    public bool IsNewBrowserLogin { get; set; }
    public U2FAuthenticationRequestDTO U2FAuthenticationRequest { get; set; }
    public bool PasswordResetRequired { get; set; }
    public PasswordResetRequestDTO PasswordResetRequest { get; set; }
}
