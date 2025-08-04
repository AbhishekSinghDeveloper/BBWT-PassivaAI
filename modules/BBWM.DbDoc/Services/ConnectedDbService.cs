using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.DbDoc.DbSchemas.DTO;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DbSchemas.SchemaModels;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Extensions;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;
using Microsoft.EntityFrameworkCore;

namespace BBWM.DbDoc.Services;

public class ConnectedDbService : IConnectedDbService
{
    private readonly IDbSchemaManager dbSchemaManager;
    private readonly IDbDocFolderService dbDocFolderService;
    private readonly IDbContext context;

    public ConnectedDbService(
        IDbSchemaManager dbSchemaManager,
        IDbDocFolderService dbDocFolderService,
        IDbContext context)
    {
        this.dbSchemaManager = dbSchemaManager;
        this.dbDocFolderService = dbDocFolderService;
        this.context = context;
    }

    public async Task<Guid> CreateFolderByDbConnection(CreateFolderByDbConnectionRequest addRequest, CancellationToken ct)
    {
        var dbSchema = await dbSchemaManager.RegisterNewDatabaseSource(
            new DatabaseSourceRegisterRequest
            {
                DatabaseType = addRequest.DatabaseType,
                ConnectionString = addRequest.ConnectionString,
                ContextId = addRequest.ContextId,
            }, ct);

        var folder = new Folder
        {
            DatabaseSourceId = dbSchema.DatabaseSource.Id,
            Name = addRequest.FolderName,
            ChangedOn = DateTime.UtcNow,
            Description = addRequest.FolderDescription,
            IsSourceFolder = true
        };

        var resultFolder = await CreateFolder(folder, dbSchema, ct);
        return resultFolder.Id;
    }

    public async Task<FolderDTO> SyncFolderFromDatabaseSource(Guid folderId, CancellationToken ct)
    {
        var dbSourceId = await context.Set<Folder>()
            .Include(x => x.DatabaseSource)
            .Where(x => x.Id == folderId)
            .Select(x => x.DatabaseSourceId)
            .SingleOrDefaultAsync(ct)
            ?? throw new ObjectNotExistsException($"The folder with ID '{folderId}' doesn't have database source.");

        var reloadedDbSchema = await dbSchemaManager.ReloadDatabaseSource(dbSourceId, ct);

        await SyncFolderFromDbSchema(folderId, reloadedDbSchema, ct);

        return await dbDocFolderService.GetFolder(folderId, ct);
    }

    /// <summary>
    /// TODO: for now this sync method fully re-creates tables of synced folder and so table/columns
    /// metadata of the folder get lost. In improved version it should sync to remain existing metadata.
    /// Also with current version it's very slow because of full re-creating.
    /// </summary>
    /// <param name="folderId"></param>
    /// <param name="reloadedDbSchema"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task SyncFolderFromDbSchema(Guid folderId, DbSchema dbSchema, CancellationToken ct)
    {
        await dbDocFolderService.RemoveFolderTables(folderId, ct);
        var folder = await context.Set<Folder>().SingleOrDefaultAsync(x => x.Id == folderId, ct);
        await CreateFolderTables(folder, dbSchema, ct);
        await context.SaveChangesAsync(ct);
    }

    private async Task<Folder> CreateFolder(Folder folder, DbSchema dbSchema, CancellationToken ct)
    {
        await context.Set<Folder>().AddAsync(folder, ct);
        await CreateFolderTables(folder, dbSchema, ct);
        await context.SaveChangesAsync(ct);

        return folder;
    }

    private async Task CreateFolderTables(Folder folder, DbSchema dbSchema, CancellationToken ct)
    {
        foreach (var tableKeyValuePair in dbSchema.Tables)
        {
            var tableMetadata = new TableMetadata
            {
                TableId = tableKeyValuePair.Key,
                Folder = folder,
                Columns = dbSchema.Columns
                    .Where(x => x.Value.TableId == tableKeyValuePair.Key)
                    .Select(x => new ColumnMetadata { ColumnId = x.Key }.FromSchemaColumn(x.Value))
                    .ToList()
            };

            await context.Set<TableMetadata>().AddAsync(tableMetadata, ct);
        }
    }

}
