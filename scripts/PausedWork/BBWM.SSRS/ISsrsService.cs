using System;
using System.Threading;
using System.Threading.Tasks;
namespace BBWM.SSRS

{
    public interface ISsrsService
    {
        Task<ReportAccessDataDTO> GetReportToken(string currentUserId, Guid reportId, CancellationToken cancellationToken = default);
    }
}