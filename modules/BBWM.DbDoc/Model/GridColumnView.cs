using BBWM.Core.Data;

namespace BBWM.DbDoc.Model;

public class GridColumnView : IAuditableEntity
{
    public int Id { get; set; }

    public float? MinWidth { get; set; }

    public float? MaxWidth { get; set; }

    public string Mask { get; set; }


    public int ColumnViewMetadataId { get; set; }

    public ColumnViewMetadata ColumnViewMetadata { get; set; }
}
