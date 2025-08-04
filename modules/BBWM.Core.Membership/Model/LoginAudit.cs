using BBWM.Core.Data;

namespace BBWM.Core.Membership.Model;

public class LoginAudit : IEntity
{
    public int Id { get; set; }
    public DateTimeOffset Datetime { get; set; }
    public string Email { get; set; }
    public string Ip { get; set; }
    // Calculation of "Location" field is not supported in the template code. It's up to a customer project to implement it.
    // Nevertheless, we do remain a corresponding field in the login audits model (and corresponding DB table's field).
    public string Location { get; set; }
    public string Fingerprint { get; set; }
    public string Browser { get; set; }
    public string Result { get; set; }
}
