using BBF.Reporting.Widget.ControlSet.DTO;

namespace BBF.Reporting.Widget.ControlSet.Interfaces;

public interface IWidgetControlSetViewService
{
    Task<ControlSetDisplayViewDTO> GetView(Guid widgetSourceId, CancellationToken ct = default);
}
