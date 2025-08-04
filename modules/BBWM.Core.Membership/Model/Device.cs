using BBWM.Core.Data;

namespace BBWM.Core.Membership.Model;

public class Device : IAuditableEntity
{
    public int Id { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public byte[] KeyHandle { get; set; }

    public byte[] PublicKey { get; set; }

    public byte[] AttestationCert { get; set; }

    public int Counter { get; set; }

    public bool IsCompromised { get; set; }
}
