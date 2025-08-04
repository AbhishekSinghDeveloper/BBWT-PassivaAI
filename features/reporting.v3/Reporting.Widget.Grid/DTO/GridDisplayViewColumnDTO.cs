using BBF.Reporting.Core.Enums;
using System.Text.Json.Nodes;
using BBF.Reporting.Widget.Grid.Enums;
using BBWM.Core.DTO;

namespace BBF.Reporting.Widget.Grid.DTO;

public class GridDisplayViewColumnDTO : IDTO
{
    public int Id { get; set; }

    public DataType DataType { get; set; }
    public DisplayMode DisplayMode { get; set; }
    public JsonNode ExtraSettings { get; set; } = null!;
    public JsonNode Footer { get; set; } = null!;
    public string? Header { get; set; }
    public InputType InputType { get; set; }

    /// <summary>
    /// Alias links grid column to query column
    /// </summary>
    public string? QueryAlias { get; set; }

    public int SortOrder { get; set; }
    public bool Sortable { get; set; }
    public bool Visible { get; set; }

    // Foreign keys and navigational properties.
    public string? VariableName { get; set; }
}