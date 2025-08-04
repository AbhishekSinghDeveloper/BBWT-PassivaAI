using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.AggregatedLogs;

[Produces("application/json")]
[Route("api/log")]
[ReadWriteAuthorize(ReadRoles = Core.Roles.SystemAdminRole + "," + Core.Roles.SuperAdminRole)]
public class LogController : DataControllerBase<ILogContext, Log, LogDTO, LogDTO, int>
{
    public LogController(IDataService<ILogContext> dataService, ILogService logService)
        : base(dataService, logService)
    {
    }
}
