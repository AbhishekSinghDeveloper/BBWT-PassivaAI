using BBF.Reporting.TableSet.DTO;
using BBF.Reporting.TableSet.Interfaces;
using BBWM.Core.Services;
using BBWM.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.TableSet.Api;

[Route("api/reporting3/query/table-set")]
[Authorize]
public class TableSetController : DataControllerBase<DbModel.TableSet, TableSetDTO, TableSetDTO>
{
    private readonly ITableSetService _tableSetService;

    public TableSetController(ITableSetService tableSetService, IDataService dataService) : base(dataService)
        => _tableSetService = tableSetService;

    [HttpGet("folders")]
    public async Task<IActionResult> GetFolders(CancellationToken ct = default)
        => Ok(await _tableSetService.GetFolders(ct));


    [HttpGet("{sourceCode}/{folderId}/tables")]
    public async Task<IActionResult> GetFolderTables(string sourceCode, string folderId, CancellationToken ct = default)
        => Ok(await _tableSetService.GetFolderTables(sourceCode, folderId, ct));

    [HttpGet("{sourceCode}/{folderId}/{tableId}/table")]
    public async Task<IActionResult> GetTable(string sourceCode, string folderId, string tableId,
        [FromQuery] string? parentTableId = null, CancellationToken ct = default)
        => Ok(await _tableSetService.GetTable(sourceCode, folderId, tableId, parentTableId, ct));

    [HttpGet("{sourceCode}/{folderId}/{tableId}/columns")]
    public async Task<IActionResult> GetTableColumns(string sourceCode, string folderId, string tableId,
        [FromQuery] string? parentTableId = null, CancellationToken ct = default)
        => Ok(await _tableSetService.GetTableColumns(sourceCode, folderId, tableId, parentTableId, ct));
}