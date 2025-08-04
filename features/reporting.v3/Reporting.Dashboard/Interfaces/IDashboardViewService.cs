using BBF.Reporting.Dashboard.DTO;

namespace BBF.Reporting.Dashboard.Interfaces;

public interface IDashboardViewService
{
    Task<DashboardViewDTO> GetView(Guid dashboardId, CancellationToken ct = default);
    Task<DashboardViewDTO> GetViewByCode(string urlSlug, CancellationToken ct);
}
