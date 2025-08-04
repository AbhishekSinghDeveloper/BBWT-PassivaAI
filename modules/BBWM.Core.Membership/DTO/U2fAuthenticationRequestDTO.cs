namespace BBWM.Core.Membership.DTO;

public class U2FAuthenticationRequestDTO
{
    public string AppId { get; set; }
    public string Version { get; set; }
    public List<U2FRegisteredKeysDTO> RegisteredKeys { get; set; }
    public string Challenge { get; set; }

    public U2FAuthenticationRequestDTO()
    {
        RegisteredKeys = new List<U2FRegisteredKeysDTO>();
    }
}
