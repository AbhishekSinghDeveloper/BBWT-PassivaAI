using BBF.Reporting.Core.DTO;
using BBWM.Core.DTO;

namespace BBF.Reporting.Widget.ControlSet.DTO;

public class ControlSetViewDTO : IDTO
{
    public int Id { get; set; }

    // Foreign keys and navigational properties.
    public Guid WidgetSourceId { get; set; }

    public WidgetSourceDTO WidgetSource { get; set; } = null!;

    public IList<ControlSetViewItemDTO> Items { get; set; } = new List<ControlSetViewItemDTO>();
}