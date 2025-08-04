using BBWM.DbDoc.DbSchemas.SchemaModels;
using BBWM.DbDoc.Enums;

namespace BBWM.DbDoc.DTO;

public class ColumnMetadataDTO
{
    public int Id { get; set; }

    public string ColumnId { get; set; }

    public bool Hidden { get; set; }

    public AnonymizationRule? AnonymizationRule { get; set; }

    public string Description { get; set; }

    /// <summary>
    /// Field representation for UI's title, header, placeholder etc.
    /// </summary>
    public string Title { get; set; }

    //TODO: rename to SchemaColumn
    public DbSchemaColumn StaticData { get; set; }


    public int? TableId { get; set; }

    public int? ViewMetadataId { get; set; }

    public ColumnViewMetadataDTO ViewMetadata { get; set; }

    public int? ValidationMetadataId { get; set; }

    public ColumnValidationMetadataDTO ValidationMetadata { get; set; }

    public Guid? ColumnTypeId { get; set; }

    /// <summary>
    /// Link to a Custom Column Type.
    /// </summary>
    public ColumnTypeDTO ColumnType { get; set; }
}
