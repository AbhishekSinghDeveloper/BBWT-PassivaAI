using BBWM.Core.Data;

namespace BBF.Reporting.TableSet.DbModel;

public class TableSet : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// A short code representing table schema's source.
    /// Possible sources now: "dbdoc" of DB Documenting module, "form" of the Forms module.
    /// </summary>
    public string FolderSourceCode { get; set; } = null!;

    /// <summary>
    /// Folder of tables source, selected by default in the tables selector component.
    /// When null, the table source provides default folder's tables.
    /// </summary>
    public string? FolderId { get; set; }
}