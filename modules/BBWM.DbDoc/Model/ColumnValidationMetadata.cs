using BBWM.Core.Data;

namespace BBWM.DbDoc.Model;

public class ColumnValidationMetadata : IAuditableEntity
{
    public int Id { get; set; }

    public string Rules { get; set; }
}
