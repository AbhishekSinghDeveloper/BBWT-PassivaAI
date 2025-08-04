namespace BBWM.Core.Membership.DTO;

public class U2FRegisteredKeysDTO
{
    public string Challenge { get; set; }
    public string Version { get; set; }
    public string KeyHandle { get; set; }
}
