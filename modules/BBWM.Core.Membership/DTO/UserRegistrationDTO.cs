namespace BBWM.Core.Membership.DTO;

public class UserRegistrationDTO
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; }
    public string PasswordSHA1 { get; set; }
    public int? OrganizationId { get; set; }
}
