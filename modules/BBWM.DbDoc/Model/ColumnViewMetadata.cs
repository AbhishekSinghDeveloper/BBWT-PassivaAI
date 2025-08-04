using BBWM.Core.Data;

namespace BBWM.DbDoc.Model;

public class ColumnViewMetadata : IAuditableEntity
{
    public int Id { get; set; }


    public GridColumnView GridColumnView { get; set; }
}
