using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Enums;
using BBWM.Core.DTO;

namespace BBF.Reporting.Widget.Grid.DTO;

public class GridViewDTO : IDTO
{
    public int Id { get; set; }

    public bool IsRowSelectable { get; set; }
    public bool SummaryFooterVisible { get; set; }
    public SortOrder DefaultSortOrder { get; set; }
    public string? DefaultSortColumnAlias { get; set; }
    public bool ShowVisibleColumnsSelector { get; set; }

    // Foreign keys and navigational properties.
    public Guid? QuerySourceId { get; set; }
    public Guid WidgetSourceId { get; set; }

    public WidgetSourceDTO WidgetSource { get; set; } = null!;

    public IList<GridViewColumnDTO> Columns { get; set; } = new List<GridViewColumnDTO>();
}