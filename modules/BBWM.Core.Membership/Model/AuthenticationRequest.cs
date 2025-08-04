using BBWM.Core.Data;

namespace BBWM.Core.Membership.Model;

public class AuthenticationRequest : IAuditableEntity
{
    public int Id { get; set; }

    public string KeyHandle { get; set; }

    public string Challenge { get; set; }

    public string AppId { get; set; }

    public string Version { get; set; }
}
