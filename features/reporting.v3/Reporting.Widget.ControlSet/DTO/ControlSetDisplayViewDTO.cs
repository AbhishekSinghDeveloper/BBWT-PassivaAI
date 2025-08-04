using BBF.Reporting.Core.DTO;
using BBWM.Core.DTO;

namespace BBF.Reporting.Widget.ControlSet.DTO;

public class ControlSetDisplayViewDTO : IDTO
{
    public int Id { get; set; }

    // Foreign keys and navigational properties.
    public Guid WidgetSourceId { get; set; }

    public WidgetSourceDTO WidgetSource { get; set; } = null!;

    public IList<ControlSetDisplayViewItemDTO> Items { get; set; } = new List<ControlSetDisplayViewItemDTO>();
}