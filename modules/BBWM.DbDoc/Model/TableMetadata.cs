using BBWM.Core.Data;
using BBWM.DbDoc.Enums;

namespace BBWM.DbDoc.Model;

/// <summary>
/// Represents metadata for a DB table.
/// </summary>
public class TableMetadata : IAuditableEntity
{
    public int Id { get; set; }

    public string TableId { get; set; }

    public string Description { get; set; }

    public AnonymizationAction? Anonymization { get; set; }

    public string Representation { get; set; }


    public Guid FolderId { get; set; }

    public Folder Folder { get; set; }

    public ICollection<ColumnMetadata> Columns { get; set; } = new List<ColumnMetadata>();
}
