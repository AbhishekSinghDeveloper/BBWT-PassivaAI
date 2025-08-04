using BBF.Reporting.Widget.Chart.DTO;

namespace BBF.Reporting.Widget.Chart.Interfaces;

public interface IWidgetChartViewService
{
    Task<ChartViewDTO> GetView(Guid widgetSourceId, CancellationToken ct = default);
}