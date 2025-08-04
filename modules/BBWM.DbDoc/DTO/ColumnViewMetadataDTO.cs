using BBWM.Core.DTO;

namespace BBWM.DbDoc.DTO;

public class ColumnViewMetadataDTO : IDTO
{
    public int Id { get; set; }


    public GridColumnViewDTO GridColumnView { get; set; }
}
