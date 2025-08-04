namespace BBWM.DbDoc.DTO;

public class SaveTableEntityRequest
{
    public int TableMetadataId { get; set; }

    public dynamic Entity { get; set; }
}
