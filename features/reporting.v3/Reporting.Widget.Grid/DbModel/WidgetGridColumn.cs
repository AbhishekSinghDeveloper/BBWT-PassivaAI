using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.Enums;
using BBWM.Core.Data;
using BBF.Reporting.Widget.Grid.Enums;

namespace BBF.Reporting.Widget.Grid.DbModel;

public class WidgetGridColumn : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Implicitly it references to DB DOC custom column type, but widget should not be linked to
    /// query's DB schema and therefore shouldn't to DB DOC. Therefore we don't have FK to the DB DOC' table
    /// </summary>
    public Guid? CustomColumnTypeId { get; set; }

    public DataType DataType { get; set; }
    public DisplayMode DisplayMode { get; set; }
    public string? ExtraSettings { get; set; }
    public string? Footer { get; set; }
    public string? Header { get; set; }
    public bool InheritHeader { get; set; }
    public InputType InputType { get; set; }

    /// <summary>
    /// Alias links grid column to query column
    /// </summary>
    public string? QueryAlias { get; set; }

    public int SortOrder { get; set; }
    public bool Sortable { get; set; }
    public bool Visible { get; set; }

    // Foreign keys and navigational properties.
    public int? VariableId { get; set; }
    public int GridId { get; set; }

    public Variable? Variable { get; set; }
    public WidgetGrid Grid { get; set; } = null!;
}