using BBWM.Core.Filters;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BBWM.DbDoc.Api;

[Route("api/db-doc/table-data")]
[Authorize(Roles = BBWM.Core.Roles.SuperAdminRole)]
public class DbDocTableDataController : BBWM.Core.Web.ControllerBase
{
    private readonly IDbDocPagedGridService _dbDocPagedGridService;


    public DbDocTableDataController(IDbDocPagedGridService dbDocPagedGridService)
        => _dbDocPagedGridService = dbDocPagedGridService;

    [HttpGet("table-data-view-settings")]
    public IActionResult GetTableDataViewSettings([FromServices] IOptions<DbDocSettings> dbDocSettings)
        => Ok(new TableDataViewSettings
        {
            ReadOnlyTableData = dbDocSettings.Value.ReadOnlyTableData,
            ShowTableData = dbDocSettings.Value.ShowTableData
        });

    [HttpGet, Route("table/{tableMetadataId}/{folderId}"), ResponseCache(NoStore = true)]
    public async Task<JsonResult> GetTableRowsPage(string tableMetadataId, Guid folderId,
        [FromQuery] QueryCommand command, CancellationToken ct)
        => Json(await _dbDocPagedGridService.GetPage(tableMetadataId, folderId, command, ct));

    [HttpPost("save-table-entity")]
    public async Task<JsonResult> SaveTableEntity([FromBody] SaveTableEntityRequest request, CancellationToken ct)
        => Json(await _dbDocPagedGridService.UpdateRow(request.Entity, request.TableMetadataId, ct));

    [HttpPost("delete-table-entity")]
    public async Task<IActionResult> DeleteTableEntity([FromBody] DeleteTableEntityRequest request, CancellationToken ct)
    {
        if (int.TryParse(request.EntityId, out int intEntityId))
        {
            await _dbDocPagedGridService.DeleteRow(request.UniqueTableId, intEntityId, ct);
            return NoContent();
        }

        await _dbDocPagedGridService.DeleteRow(request.UniqueTableId, request.EntityId, ct);
        return NoContent();
    }
}
