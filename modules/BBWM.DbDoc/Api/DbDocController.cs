using BBWM.Core.AppEnvironment;
using BBWM.Core.Exceptions;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.DbDoc.Api;

[Route("api/db-doc")]
[Authorize(Roles = BBWM.Core.Roles.SuperAdminRole)]
public class DbDocController : BBWM.Core.Web.ControllerBase
{
    private readonly IDbDocService _dbDocService;


    public DbDocController(IDbDocService dbDocService) =>
        _dbDocService = dbDocService;

    [HttpPost("copy-table-metadata-to-folder")]
    public async Task<IActionResult> CopyTableMetadataToFolder([FromBody] CopyTableMetadataToFolderDTO dto, CancellationToken ct) =>
        Ok(await _dbDocService.CopyTableMetadataToFolder(dto, ct));

    [HttpDelete("delete-table-metadata/{tableMetadataId}")]
    public async Task<IActionResult> DeleteTableMetadata(int tableMetadataId, CancellationToken ct)
    {
        if (!await _dbDocService.TableMetadataExists(tableMetadataId, ct))
            throw new EntityNotFoundException($"The table metadata item with ID '{tableMetadataId}' not found.");

        var folder = await _dbDocService.DeleteTableMetadata(tableMetadataId, ct);
        return Ok(folder);
    }

    [HttpPost("delete-column-validation-metadata/{columnMetadataId}")]
    public async Task<IActionResult> DeleteColumnValidationMetadata(int columnMetadataId, CancellationToken ct)
    {
        if (!await _dbDocService.ColumnMetadataExists(columnMetadataId, ct))
            throw new EntityNotFoundException($"The column metadata item with ID '{columnMetadataId}' not found.");

        await _dbDocService.DeleteColumnValidationMetadata(columnMetadataId, ct);
        return NoContent();
    }

    [HttpPost("delete-column-view-metadata/{columnMetadataId}")]
    public async Task<IActionResult> DeleteColumnViewMetadata(int columnMetadataId, CancellationToken ct)
    {
        if (!await _dbDocService.ColumnMetadataExists(columnMetadataId, ct))
            throw new EntityNotFoundException($"The column metadata item with ID '{columnMetadataId}' not found.");

        await _dbDocService.DeleteColumnViewMetadata(columnMetadataId, ct);
        return NoContent();
    }

    [HttpGet("get-table-metadata/{tableUniqueId}/{folderId?}")]
    [Authorize]
    public async Task<IActionResult> GetTableMetadata(string tableUniqueId, Guid? folderId, CancellationToken ct) =>
        Ok(await _dbDocService.GetActualTableMetadata(tableUniqueId, folderId, ct));

    [HttpPost("set-column-type-metadata-for-column-metadata")]
    public async Task<IActionResult> SetColumnTypeMetadataForColumnMetadata([FromBody] SetColumnTypeMetadataForColumnMetadataRequest dto, CancellationToken ct = default) =>
        Ok(await _dbDocService.SetColumnTypeMetadataForColumnMetadata(dto.ColumnMetadataId, dto.ColumnTypeId, ct));

    [HttpPost("set-column-validation-metadata/{columnMetadataId}")]
    public async Task<IActionResult> SetColumnValidationMetadata(int columnMetadataId, [FromBody] ColumnValidationMetadataDTO columnValidationMetadata, CancellationToken ct) =>
        await _dbDocService.ColumnMetadataExists(columnMetadataId, ct)
            ? Ok(await _dbDocService.SetValidationMetadata(columnMetadataId, columnValidationMetadata, ct))
            : throw new EntityNotFoundException($"The column metadata item with ID '{columnMetadataId}' not found.");

    [HttpPost("set-column-view-metadata/{columnMetadataId}")]
    public async Task<IActionResult> SetColumnViewMetadata(int columnMetadataId, [FromBody] ColumnViewMetadataDTO columnViewMetadata, CancellationToken ct) =>
        await _dbDocService.ColumnMetadataExists(columnMetadataId, ct)
            ? Ok(await _dbDocService.SetViewMetadata(columnMetadataId, columnViewMetadata, ct))
            : throw new EntityNotFoundException($"The column metadata item with ID '{columnMetadataId}' not found.");

    [HttpPut("update-column-metadata/{columnMetadataId}")]
    public async Task<IActionResult> UpdateColumnMetadata(int columnMetadataId, [FromBody] ColumnMetadataDTO dto, CancellationToken ct) =>
        await _dbDocService.ColumnMetadataExists(columnMetadataId, ct)
            ? Ok(await _dbDocService.UpdateColumnMetadata(columnMetadataId, dto, ct))
            : throw new EntityNotFoundException($"The column metadata item with ID '{columnMetadataId}' not found.");

    [HttpPut("update-table-metadata/{tableMetadataId}")]
    public async Task<IActionResult> UpdateTableMetadata(int tableMetadataId, [FromBody] TableMetadataDTO dto, CancellationToken ct) =>
        await _dbDocService.TableMetadataExists(tableMetadataId, ct)
            ? Ok(await _dbDocService.UpdateTableMetadata(tableMetadataId, dto, ct))
            : throw new EntityNotFoundException($"The table metadata item with ID '{tableMetadataId}' not found.");

    [HttpGet("anonymization-xml/{folderId}"), ResponseCache(NoStore = true)]
    public async Task<FileContentResult> GetAnonymizationXml(Guid folderId,
        [FromServices] IDbDocAnonymizeService dbDocAnonymizeService, CancellationToken ct) =>
        File(await dbDocAnonymizeService.GetAnonymizationXml(folderId, ct), "text/xml", "anonymization.xml");

    /// <summary>
    /// This is a temp. method that cannot/should not be used in live project, for good. Only for superadmin to fix
    /// MySQL case (from precedents and only specific for MySQL): when mysql migration was executed but migration record
    /// not added. 
    /// </summary>
    [HttpGet("temp-add-mysql-migration/{name}/{version}")]
    [Authorize(Roles = BBWM.Core.Roles.SuperAdminRole)]
    public async Task<IActionResult> TempMethodAddMySqlMigration(
        [FromServices] IDbSchemaManager dbSchemaManager, string name, string version, CancellationToken ct)
    {
        // Only allowing this hack for test/development environments...
        if (!AppEnvironment.IsDevelopment && !AppEnvironment.IsTest)
            throw new Exception("Not supported environment");

        await using var connection = new MySqlConnector.MySqlConnection(
            dbSchemaManager.GetMainDbSchema().DatabaseSource.ConnectionString);
        await connection.OpenAsync(ct);

        var sql0 = $"SELECT 1 FROM __efmigrationshistory where MigrationId = '{name}'";
        await using var command0 = new MySqlConnector.MySqlCommand(sql0, connection);
        var res0 = await command0.ExecuteScalarAsync(ct);

        if (res0?.ToString() == "1")
        {
            return Ok("Already exists");
        }

        var sqlCode = $"INSERT INTO __efmigrationshistory (MigrationId, ProductVersion) Values ('{name}', '{version}')";
        await using var command = new MySqlConnector.MySqlCommand(sqlCode, connection);
        var res = await command.ExecuteNonQueryAsync(ct);

        return Ok(res);
    }
}