using BBWM.Core.DTO;
using BBWM.DbDoc.Enums;

namespace BBWM.DbDoc.DTO;

public class ColumnTypeDTO : IDTO<Guid>
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public ClrTypeGroup Group { get; set; }

    public AnonymizationRule? AnonymizationRule { get; set; }



    public int? ViewMetadataId { get; set; }

    public ColumnViewMetadataDTO ViewMetadata { get; set; }

    public int? ValidationMetadataId { get; set; }

    public ColumnValidationMetadataDTO ValidationMetadata { get; set; }
}
