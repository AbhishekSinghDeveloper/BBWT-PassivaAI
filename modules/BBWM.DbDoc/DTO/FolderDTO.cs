namespace BBWM.DbDoc.DTO;

public class FolderDTO
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public DateTime ChangedOn { get; set; }

    public string Description { get; set; }

    public Guid? DatabaseSourceId { get; set; }

    public IList<string> Owners { get; set; }

    public bool IsSourceFolder { get; set; }

    /// <summary>
    /// Protected folder is supposed to be restricted from deleting operation.
    /// This restriction is not applied to:
    ///     - refreshing folder's DB schema by re-scanning connected DB;
    ///     - tables/columns metadata modifications (e.g. adding column formatting rules).
    /// </summary>
    public bool Protected { get; set; }

    public DatabaseSourceDetailsDTO DatabaseSource { get; set; }

    public IList<TableMetadataDTO> Tables { get; set; } = new List<TableMetadataDTO>();
}
