using BBWM.SystemData.DTO;

namespace BBWM.SystemData;

public interface ISystemDataService
{
    Task<SystemSummaryDTO> GetSystemSummary();
    VersionInfoDTO GetVersionInfo();
    DebugInfoDTO GetDebugInfo();
    IEnumerable<OutputExceptionDTO> GetApiExceptions();
}