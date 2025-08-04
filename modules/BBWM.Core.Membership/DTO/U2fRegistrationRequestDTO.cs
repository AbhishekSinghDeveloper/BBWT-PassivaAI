namespace BBWM.Core.Membership.DTO;

public class U2FRegistrationRequestDTO
{
    public string AppId { get; set; }
    public string Challenge { get; set; }
    public string Version { get; set; }
}
