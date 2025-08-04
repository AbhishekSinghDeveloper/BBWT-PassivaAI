using BBWM.DbDoc.DbSchemas.SchemaModels;
using BBWM.DbDoc.Enums;

namespace BBWM.DbDoc.DTO;

public class TableMetadataDTO
{
    public int Id { get; set; }

    public string TableId { get; set; }

    public string Description { get; set; }

    public AnonymizationAction? Anonymization { get; set; }

    public string Representation { get; set; }

    //TODO: rename to SchemaTable
    public DbSchemaTable StaticData { get; set; }


    public Guid FolderId { get; set; }

    public IList<ColumnMetadataDTO> Columns { get; set; } = new List<ColumnMetadataDTO>();
}
