using BBF.Reporting.Dashboard.DTO;
using BBWM.Core.Services;

namespace BBF.Reporting.Dashboard.Interfaces;

public interface IDashboardBuilderService : IEntityUpdate<DashboardBuildDTO>, IEntityCreate<DashboardBuildDTO>
{
    Task<DashboardBuildDTO> Create(DashboardBuildDTO dto, string userId, CancellationToken ct = default);
    Task<DashboardBuildDTO> GetBuild(Guid dashboardId, CancellationToken ct = default);
}