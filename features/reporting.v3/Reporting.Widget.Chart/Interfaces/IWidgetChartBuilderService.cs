using BBF.Reporting.Widget.Chart.DTO;
using BBWM.Core.Services;

namespace BBF.Reporting.Widget.Chart.Interfaces;

public interface IWidgetChartBuilderService : IEntityCreate<ChartBuildDTO>, IEntityUpdate<ChartBuildDTO>
{
    Task<ChartBuildDTO> Create(ChartBuildDTO build, string? userId, CancellationToken ct = default);

    Task<ChartBuildDTO> CreateDraft(ChartBuildDTO build, Guid? releaseWidgetId = null, CancellationToken ct = default);

    Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default);
}