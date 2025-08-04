using BBWM.Core.Exceptions;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.DbDoc.Api;

[Route("api/db-doc/folder")]
[Authorize(Roles = BBWM.Core.Roles.SuperAdminRole)]
public class DbDocFolderController : BBWM.Core.Web.ControllerBase
{
    private readonly IDbDocFolderService _dbDocFolderService;


    public DbDocFolderController(IDbDocFolderService dbDocFolderService) =>
        _dbDocFolderService = dbDocFolderService;


    [HttpPost]
    public async Task<IActionResult> CreateFolder([FromBody] FolderDTO folder, CancellationToken ct) =>
        Ok(await _dbDocFolderService.CreateFolder(folder, ct));


    [HttpPost("create-from-db")]
    public async Task<IActionResult> CreateFolderFromDb(
            [FromBody] CreateFolderByDbConnectionRequest request,
            [FromServices] IConnectedDbService connectedDbService,
            CancellationToken ct)
    {
        var folderId = await connectedDbService.CreateFolderByDbConnection(request, ct);
        return Ok(await _dbDocFolderService.GetFolder(folderId, ct));
    }

    [HttpPut("sync-from-db/{folderId}")]
    public async Task<IActionResult> SyncFolderFromDb(Guid folderId,
        [FromServices] IConnectedDbService connectedDbService, CancellationToken ct) =>
        Ok(await connectedDbService.SyncFolderFromDatabaseSource(folderId, ct));

    [HttpDelete("{folderId}")]
    public async Task<IActionResult> DeleteFolder(Guid folderId, CancellationToken ct)
    {
        if (!await _dbDocFolderService.FolderExists(folderId, ct))
            throw new EntityNotFoundException($"The folder with ID '{folderId}' not found.");

        var affectedFolders = await _dbDocFolderService.DeleteFolder(folderId, ct);
        return Ok(affectedFolders);
    }

    [HttpGet("{folderId}")]
    public async Task<IActionResult> GetFolder(Guid folderId, CancellationToken ct) =>
        Ok(await _dbDocFolderService.GetFolder(folderId, ct));

    [HttpGet]
    public async Task<IActionResult> GetDbExplorerFolders(CancellationToken ct) =>
        Ok(await _dbDocFolderService.GetDbExplorerFolders(ct));

    [HttpPut("{folderId}")]
    public async Task<IActionResult> UpdateFolder(Guid folderId, [FromBody] FolderDTO folder, CancellationToken ct) =>
        await _dbDocFolderService.FolderExists(folderId, ct)
            ? Ok(await _dbDocFolderService.UpdateFolder(folderId, folder, ct))
            : throw new EntityNotFoundException($"The folder with ID '{folderId}' not found.");

    [HttpGet("owner-types"), ResponseCache(NoStore = true)]
    public IActionResult GetFolderOwnerTypes() =>
        Ok(DbDocFolderOwnersRegister.GetAllOwners());

    [HttpGet("db-path-macros/{folderId}")]
    public async Task<IActionResult> GetDbPathMacrosAllAliases(Guid folderId, CancellationToken ct) =>
        Ok(await _dbDocFolderService.GetDbPathMacrosAllAliases(folderId, ct));
}