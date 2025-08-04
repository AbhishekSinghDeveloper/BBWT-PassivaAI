using BBWM.Core.DTO;

namespace BBWM.Core.Membership.DTO;

public class ActivationTokenDTO : IDTO
{
    public int Id { get; set; }
    public string Token { get; set; }
    public DateTime? ExpirationDate { get; set; }
}
