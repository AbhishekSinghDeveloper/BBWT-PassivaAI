using BBWM.Core.Data;

namespace BBWM.DbDoc.Model;

/// <summary>
/// Represents a container folder for tables set.
/// </summary>
public class Folder : IAuditableEntity<Guid>
{
    public Guid Id { get; set; }

    public DateTime ChangedOn { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Owners { get; set; }

    /// <summary>
    /// When True - a folder contains a full source DB schema. The folder's table list is readonly. It's only possible
    /// to change tables/columns metadata.
    /// When False - a folder is created from scratch, is editable and contains copies of tables from the source folder.
    /// </summary>
    public bool IsSourceFolder { get; set; }

    public Guid? DatabaseSourceId { get; set; }

    public DatabaseSource DatabaseSource { get; set; }

    public ICollection<TableMetadata> Tables { get; set; } = new List<TableMetadata>();
}