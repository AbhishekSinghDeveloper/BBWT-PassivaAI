using BBWM.Core.DTO;

namespace BBWM.Core.Membership.DTO;

public class LoginAuditDTO : IDTO
{
    public int Id { get; set; }
    public DateTimeOffset Datetime { get; set; }
    public string Ip { get; set; }
    public string Location { get; set; }
    public string Fingerprint { get; set; }
    public string Email { get; set; }
    public string Browser { get; set; }
    public string Result { get; set; }
}
