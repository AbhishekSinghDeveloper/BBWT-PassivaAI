using BBWM.Core.Data;
using BBWM.DbDoc.Enums;

namespace BBWM.DbDoc.Model;

public class ColumnType : IAuditableEntity<Guid>

{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public ClrTypeGroup Group { get; set; }

    public AnonymizationRule? AnonymizationRule { get; set; }



    public int? ViewMetadataId { get; set; }

    public ColumnViewMetadata ViewMetadata { get; set; }

    public int? ValidationMetadataId { get; set; }

    public ColumnValidationMetadata ValidationMetadata { get; set; }
}
