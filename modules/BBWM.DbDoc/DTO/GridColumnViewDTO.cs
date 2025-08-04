using BBWM.Core.DTO;

namespace BBWM.DbDoc.DTO;

public class GridColumnViewDTO : IDTO
{
    public int Id { get; set; }

    public float? MinWidth { get; set; }

    public float? MaxWidth { get; set; }

    public string Mask { get; set; }


    public int ColumnViewMetadataId { get; set; }
}
