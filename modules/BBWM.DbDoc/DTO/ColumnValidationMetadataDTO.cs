using BBWM.Core.DTO;
using BBWM.DbDoc.Core.Classes.ValidationRules;

namespace BBWM.DbDoc.DTO;

public class ColumnValidationMetadataDTO : IDTO
{
    public int Id { get; set; }

    public ValidationRule[] Rules { get; set; }
}
