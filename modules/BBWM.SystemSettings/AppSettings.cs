using BBWM.Core.Data;

namespace BBWM.SystemSettings;

public class AppSettings : IAuditableEntity
{
    public int Id { get; set; }
    public string Section { get; set; }
    public string Value { get; set; }
    public string EncryptedFields { get; set; }
}
