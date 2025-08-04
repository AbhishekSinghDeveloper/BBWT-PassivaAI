using BBF.Reporting.Widget.Grid.DTO;

namespace BBF.Reporting.Widget.Grid.Interfaces;

public interface IWidgetGridViewService
{
    Task<GridDisplayViewDTO> GetView(Guid widgetSourceId, CancellationToken ct = default);
}
