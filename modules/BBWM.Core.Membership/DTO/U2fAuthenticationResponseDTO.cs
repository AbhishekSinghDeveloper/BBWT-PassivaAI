namespace BBWM.Core.Membership.DTO;

public class U2FAuthenticationResponseDTO
{
    public string UserId { get; set; }
    public string ClientData { get; set; }
    public string KeyHandle { get; set; }
    public string SignatureData { get; set; }
}
