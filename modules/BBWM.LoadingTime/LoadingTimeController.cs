using BBWM.Core.Filters;
using BBWM.Core.ModelHashing;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.Filters;
using BBWM.SystemSettings;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.LoadingTime;

[Route("api/loading-time")]
[ReadWriteAuthorize(ReadWriteRoles = Core.Roles.SuperAdminRole)]
public class LoadingTimeController : DataControllerBase<LoadingTime, LoadingTimeDTO, LoadingTimeDTO>
{
    private readonly ISettingsService _settingsService;

    public LoadingTimeController(IDataService dataService, ISettingsService settingsService) : base(dataService)
        => _settingsService = settingsService;

    [HttpPost]
    [Authorize]
    public override async Task<IActionResult> Create([FromBody] LoadingTimeDTO dto,
        [FromServices] IModelHashingService modelHashingService, CancellationToken ct = default)
    {
        var doRecord = _settingsService.GetSettingsSection<LoadingTimeSettings>()?.RecordLoadingTime ?? false;

        // When loading time recording is turned off then we shouldn't post new records, then this check is to
        // prevent possible flood attack by an authenticated user.
        return doRecord ? Request.CreatedResult<LoadingTimeDTO, int>(
            await DataService.Create<LoadingTime, LoadingTimeDTO>(dto, ct), modelHashingService)
            : Forbid();
    }

    [HttpGet]
    [Route("get-average")]
    public async Task<IActionResult> GetAverage(QueryCommand query, CancellationToken ct)
    {
        query.Skip = null;
        query.Take = null;

        var result = await DataService.GetPage<LoadingTime, LoadingTimeDTO>(query, ct);

        float sum = 0;
        var total = 0;
        foreach (var item in result.Items)
        {
            sum += item.Time;
            total++;
        }
        var average = sum / total;

        return Ok(average / 1000);
    }
}
