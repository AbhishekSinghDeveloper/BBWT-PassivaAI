using BBWM.DbDoc.Core.Classes.ValidationRules;

namespace BBWM.DbDoc.DTO;

public class ColumnMetadataResult
{
    public IEnumerable<ValidationRule> ValidationRules { get; set; }

    public GridColumnViewDTO GridColumnView { get; set; }
}
