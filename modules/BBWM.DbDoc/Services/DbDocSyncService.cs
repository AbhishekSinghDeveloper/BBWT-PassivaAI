using BBWM.Core.Data;
using BBWM.Core.Utils;
using BBWM.DbDoc.Core.Classes;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BBWM.DbDoc.Services;

public partial class DbDocSyncService : IDbDocSyncService
{
    public const string ContextModelDatabaseSourceName = "All model contexts";

    private readonly IDbContext _context;
    private readonly IDbDocFolderService _dbDocFolderService;
    private readonly IDbDocService _dbDocService;
    private readonly IDbSchemaManager _dbSchemaManager;
    private readonly IColumnTypeService _columnTypeService;
    private readonly IDbDocGitLabService _dbDocGitLabService;
    private readonly IConnectedDbService _connectedDbService;
    private readonly ILogger<DbDocService> _logger;
    private readonly DbDocSettings _dbDocSettings;

    private readonly string _jsonPath;

    public DbDocSyncService(
        IHostEnvironment environment,
        IDbContext context,
        IDbDocFolderService dbDocFolderService,
        IDbDocService dbDocService,
        IDbSchemaManager dbSchemaManager,
        IColumnTypeService columnTypeService,
        IDbDocGitLabService dbDocGitLabService,
        IConnectedDbService connectedDbService,
        IOptions<DbDocSettings> dbDocSettings,
        ILogger<DbDocService> logger)
    {
        _context = context;
        _dbDocFolderService = dbDocFolderService;
        _dbDocService = dbDocService;
        _dbSchemaManager = dbSchemaManager;
        _columnTypeService = columnTypeService;
        _dbDocGitLabService = dbDocGitLabService;
        _connectedDbService = connectedDbService;
        _logger = logger;
        _dbDocSettings = dbDocSettings.Value;

        _jsonPath = $"{environment.ContentRootPath}/{_dbDocSettings.FilePath}";
    }

    /// <summary>
    /// Synchronizes data from the JSON file and DB.
    /// </summary>
    public async Task Synchronize()
    {
        await SyncJsonToDatabase();

        await SeedMainDbFolder();

        await _dbDocGitLabService.SendCurrentDbDocStateToGit(true);
    }

    private async Task SyncJsonToDatabase()
    {
        var metadataFromFile = await ReadMetadataFromJsonFile();
        var metadataFromDb = await _context.Set<Folder>().ToListAsync();

        // Analyzes column types from JSON file and refreshes it for DB
        foreach (var columnType in metadataFromFile.ColumnTypes)
        {
            var existingColumnType = await _context.Set<ColumnType>()
                .SingleOrDefaultAsync(x => x.Id == columnType.Id);

            if (existingColumnType is not null)
                await _columnTypeService.Delete(existingColumnType.Id);

            await _context.Set<ColumnType>().AddAsync(columnType);
            await _context.SaveChangesAsync();
        }

        // Analyzes metadata from JSON file and refreshes it for DB
        foreach (var dbDocFolderFromFile in metadataFromFile.Folders)
        {
            var dbDocFolderFromDb = metadataFromDb
                .SingleOrDefault(x => x.Id == dbDocFolderFromFile.Id);
            if (dbDocFolderFromDb is not null)
            {
                if (dbDocFolderFromDb.ChangedOn < dbDocFolderFromFile.ChangedOn)
                    await _dbDocFolderService.DeleteFolder(dbDocFolderFromDb.Id, CancellationToken.None);
                else
                    continue;
            }

            await _context.Set<Folder>().AddAsync(dbDocFolderFromFile);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedMainDbFolder(CancellationToken ct = default)
    {
        var mainDbSourceContextId = _dbSchemaManager.MainDbContextId;

        // TODO: we need to sync tables/columns metadata records on DB schema scanning?
        // TODO: what if connection string has changed for the main DB context - need to update in the database source.

        if (await _context.Set<Folder>().AnyAsync(x => x.DatabaseSource.ContextId == mainDbSourceContextId))
            return;

        var addRequest = new DTO.CreateFolderByDbConnectionRequest
        {
            FolderName = _dbDocFolderService.DefaultFolderName,
            ContextId = mainDbSourceContextId,
            DatabaseType = _context.GetDatabaseType(),
            ConnectionString = _context.Database.GetConnectionString(),
        };

        var folderId = await _connectedDbService.CreateFolderByDbConnection(addRequest, ct);

        // TODO: fix this to a single set of folder.Owners field
        foreach (var owner in DbDocFolderOwnersRegister.GetOwnersAutoAddedToMainDbFolder())
        {
            await _dbDocFolderService.AddFolderOwner(folderId, owner, ct);
        }
    }

    private async Task<DbDocJsonStructure> ReadMetadataFromJsonFile()
    {
        if (!File.Exists(_jsonPath))
        {
            _logger?.LogWarning($"The DbDoc metadata file not found at path '{_jsonPath}'.");
            return new DbDocJsonStructure();
        }

        var fileContent = File.ReadAllText(_jsonPath);
        using FileStream openStream = File.OpenRead(_jsonPath);
        if (string.IsNullOrWhiteSpace(fileContent))
        {
            _logger?.LogWarning($"The DbDoc metadata file '{_jsonPath}' is empty.");
            return new DbDocJsonStructure();
        }

        try
        {
            DbDocJsonStructure dbDocJsonStructure =
                await JsonSerializer.DeserializeAsync<DbDocJsonStructure>(
                    openStream, JsonSerializerOptionsProvider.Options);
            return dbDocJsonStructure;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"The DbDoc metadata file '{_jsonPath}' contains corrupted data.");
            return new DbDocJsonStructure();
        }
    }
}