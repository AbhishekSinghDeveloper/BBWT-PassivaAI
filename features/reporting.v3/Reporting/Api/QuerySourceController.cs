using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Core.Model;
using BBF.Reporting.Web;
using BBWM.Core.Filters;
using BBWM.Core.Services;
using BBWM.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Api;

[Route("api/reporting3/query-source")]
[Authorize]
[NamedQuerySourceAuthorize]
public class QuerySourceController : DataControllerBase<QuerySource, QuerySourceDTO, QuerySourceDTO, Guid>
{
    private readonly IQuerySourceService _querySourceService;
    private readonly IQueryProviderFactory _queryProviderFactory;
    private readonly INamedQuerySourceService _namedQuerySourceService;

    public QuerySourceController(
        IDataService dataService,
        IQuerySourceService querySourceService,
        IQueryProviderFactory queryProviderFactory,
        INamedQuerySourceService namedQuerySourceService)
        : base(dataService, querySourceService)
    {
        _querySourceService = querySourceService;
        _queryProviderFactory = queryProviderFactory;
        _namedQuerySourceService = namedQuerySourceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(await _namedQuerySourceService.GetAll(ct));

    [HttpGet, Route("page")]
    public override async Task<IActionResult> GetPage([FromQuery] QueryCommand command, CancellationToken ct = default)
    {
        // Agreed max no of returned records.
        if (command.Take is null or > 100) command.Take = 100;
        return Ok(await _namedQuerySourceService.GetPage(command, ct));
    }

    [HttpGet("{querySourceId}/query-schema")]
    public async Task<IActionResult> GetQuerySchema(Guid querySourceId, CancellationToken ct = default)
        => Ok(await _queryProviderFactory.GetQueryProvider(querySourceId)
            ?.GetQuerySchema(querySourceId, ct)!);

    [HttpGet("{querySourceId}/query-data-rows")]
    public async Task<IActionResult> GetQueryDataRows(Guid querySourceId,
        [FromQuery] PagedGridSettings pagedGridSettings, CancellationToken ct = default)
        => Ok(await _queryProviderFactory.GetQueryProvider(querySourceId)
            ?.GetQueryDataRows(querySourceId, gridSettings: pagedGridSettings, ct: ct)!);

    [HttpGet("{querySourceId}/query-data-rows-count")]
    public async Task<IActionResult> GetQueryDataRows(Guid querySourceId, CancellationToken ct = default)
        => Ok(await _queryProviderFactory.GetQueryProvider(querySourceId)
            ?.GetQueryDataRowsCount(querySourceId, ct: ct)!);

    [HttpGet("{querySourceId}/has-attached-widgets")]
    public async Task<IActionResult> HasAttachedWidgets(Guid querySourceId, CancellationToken ct = default)
        => Ok(await _querySourceService.HasAttachedWidgets(querySourceId, ct));

    [HttpPut, Route("{querySourceId}/publish")]
    public async Task<IActionResult> PublishQuery(Guid querySourceId, [FromBody] IEnumerable<int> organizationIds,
        CancellationToken ct = default)
    {
        await _namedQuerySourceService.PublishQuery(querySourceId, organizationIds, ct);
        return Ok();
    }

    [HttpPut, Route("{querySourceId}/change-owner")]
    public async Task<IActionResult> ChangeOwner(Guid querySourceId, [FromQuery] string ownerId,
        CancellationToken ct = default)
    {
        await _namedQuerySourceService.ChangeOwner(querySourceId, ownerId, ct);
        return Ok();
    }
}