using BBWM.Core.Data;

namespace BBWT.Tests.modules.BBWM.Core.Test.Models;

public class AuditableIntPKEntity : IAuditableEntity
{
    public int Id { get; set; }

    public string Name { get; set; }
}
