using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Enums;
using BBWM.Core.DTO;

namespace BBF.Reporting.Widget.Grid.DTO;

public class GridDisplayViewDTO : IDTO
{
    public int Id { get; set; }

    public bool IsRowSelectable { get; set; }
    public bool SummaryFooterVisible { get; set; }
    public bool ShowVisibleColumnsSelector { get; set; }
    public string? DefaultSortColumnAlias { get; set; }
    public SortOrder? DefaultSortOrder { get; set; }

    // Foreign keys and navigational properties.
    public Guid? QuerySourceId { get; set; }
    public Guid WidgetSourceId { get; set; }

    public WidgetSourceDTO WidgetSource { get; set; } = null!;

    public IList<GridDisplayViewColumnDTO> Columns { get; set; } = new List<GridDisplayViewColumnDTO>();

    // Non-database properties.
    public IEnumerable<string> QueryVariables { get; set; } = new List<string>();
}