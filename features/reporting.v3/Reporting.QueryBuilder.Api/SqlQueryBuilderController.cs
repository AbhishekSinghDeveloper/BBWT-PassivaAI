using BBF.Reporting.QueryBuilder.DbModel;
using BBF.Reporting.QueryBuilder.DTO;
using BBF.Reporting.QueryBuilder.Interfaces;
using BBWM.Core.Services;
using BBWM.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.QueryBuilder.Api;

[Route("api/reporting3/query/sql")]
[Authorize]
public class SqlQueryBuilderController : DataControllerBase<SqlQuery, SqlQueryBuildDTO, SqlQueryBuildDTO>
{
    private readonly IRqbService _querySqlService;

    public SqlQueryBuilderController(IRqbService querySqlService, IDataService dataService)
        : base(dataService, querySqlService)
        => _querySqlService = querySqlService;

    [HttpGet("{querySourceId}/build")]
    public async Task<IActionResult> GetBuild(Guid querySourceId, CancellationToken ct)
        => Ok(await _querySqlService.GetBuild(querySourceId, ct));

    [HttpPost("create-draft/{querySourceReleaseId?}")]
    public async Task<IActionResult> CreateDraft(Guid? querySourceReleaseId,
        [FromBody] SqlQueryBuildDTO sqlQueryBuild, CancellationToken ct)
        => Ok(await _querySqlService.CreateDraft(sqlQueryBuild, querySourceReleaseId, ct));

    [HttpPut("release-draft/{querySourceDraftId}")]
    public async Task<IActionResult> ReleaseDraft(Guid querySourceDraftId, CancellationToken ct)
        => Ok(await _querySqlService.ReleaseDraft(querySourceDraftId, ct));
}