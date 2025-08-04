using BBWM.Core.Data;
using BBWM.DbDoc.Enums;

namespace BBWM.DbDoc.Model;

/// <summary>
/// Represents metadata for a DB column.
/// </summary>
public class ColumnMetadata : IAuditableEntity
{
    public int Id { get; set; }

    public string ColumnId { get; set; }

    /// <summary>
    /// Marks a column hidden. A column is visible by default.
    /// Visibility of a column supposes that features that take DB structure from DB Documenting tool, should handle
    /// the Hidden flag basing on a feature's logic. For example, the Reporting feature, when it displays a tables list in
    /// the query builder, will hide columns with Hidden flag from the report editor-user, supposing that these columns
    /// are not allowed for showing in the report grids for end-user.
    /// </summary>
    public bool Hidden { get; set; }

    public AnonymizationRule? AnonymizationRule { get; set; }

    public string Description { get; set; }

    /// <summary>
    /// Field representation for UI's title, header, placeholder etc.
    /// </summary>
    public string Title { get; set; }


    public int TableId { get; set; }

    public TableMetadata Table { get; set; }

    public int? ViewMetadataId { get; set; }

    public ColumnViewMetadata ViewMetadata { get; set; }

    public int? ValidationMetadataId { get; set; }

    public ColumnValidationMetadata ValidationMetadata { get; set; }

    public Guid? ColumnTypeId { get; set; }

    /// <summary>
    /// Link to a Custom Column Type.
    /// </summary>
    public ColumnType ColumnType { get; set; }
}
