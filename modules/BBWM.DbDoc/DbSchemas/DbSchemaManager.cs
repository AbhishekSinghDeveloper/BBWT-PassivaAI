using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Extensions;
using BBWM.DbDoc.DbSchemas.DTO;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DbSchemas.SchemaModels;
using BBWM.DbDoc.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BBWM.DbDoc.DbSchemas;

public class DbSchemaManager : IDbSchemaManager, IDbSchemaCodeValidator
{
    private readonly IDbContext _context;
    private readonly ILogger<DbSchemaManager> _logger;

    private IList<DbSchema> _dbSchemaStorage = new List<DbSchema>();
    private bool _isDbSchemaStorageInitialized;
    private readonly object _dbSchemaStorageLock = new();

    //TODO: to review where we should store this key?!
    private const string sensitiveDbSourceDataEncryptKey = "4tla2UmHprsI6T9FXjidI8fwKe6LmMjU";

    private string _mainDbContextId;

    public string MainDbContextId => _mainDbContextId;

    public DbSchemaManager(
        IDbContext context,
        ILogger<DbSchemaManager> logger)
    {
        _context = context;
        _mainDbContextId = _context.GetType().FullName;
        _logger = logger;
    }

    public DbSchema GetDbSchema(Guid databaseSourceId)
        => GetDbSchemaStorage().SingleOrDefault(x => x.DatabaseSource.Id == databaseSourceId);

    public DbSchema GetDbSchema(string contextId)
        => GetDbSchemaStorage().SingleOrDefault(x => x.DatabaseSource.ContextId == contextId);

    public DbSchema GetMainDbSchema()
        => GetDbSchema(MainDbContextId);

    public DbSchemaTable GetTable(string tableId)
    {
        foreach (var schema in GetDbSchemaStorage())
        {
            if (schema.Tables.TryGetValue(tableId, out var table))
                return table;
        }
        return null;
    }

    public DbSchemaColumn GetColumn(string columnId)
    {
        foreach (var schema in GetDbSchemaStorage())
        {
            if (schema.Columns.TryGetValue(columnId, out var column))
                return column;
        }
        return null;
    }

    public IEnumerable<DbSchemaColumn> GetTableColumns(string tableId) =>
        GetTableDbSchema(tableId)?.GetTableColumns(tableId) ?? new List<DbSchemaColumn>();

    public DbSchema GetTableDbSchema(string tableId) =>
        GetDbSchemaStorage().FirstOrDefault(x => x.Tables.ContainsKey(tableId));

    public async Task<DbSchema> RegisterNewDatabaseSource(DatabaseSourceRegisterRequest registerRequest,
        CancellationToken ct = default)
    {
        CheckStorageInitialized();

        // reading db schema and adding to the schemas list
        var dbSchema = ReadDbSchemaFromDbSource(registerRequest);

        var dbSource = MapRegisterRequestToDbSource(registerRequest);
        dbSource.SchemaCode = dbSchema.SchemaCode;
        await _context.Set<DatabaseSource>().AddAsync(dbSource, ct);
        await _context.SaveChangesAsync(ct);

        dbSchema.DatabaseSource = GetDecryptedDbSource(dbSource);
        AddToDbSchemaStorage(dbSchema);

        return dbSchema;
    }

    public async Task UnregisterDatabaseSource(Guid databaseSourceId, CancellationToken ct = default)
    {
        CheckStorageInitialized();

        var dbSource = await _context.Set<DatabaseSource>().SingleAsync(x => x.Id == databaseSourceId, ct);

        if (dbSource is not null)
        {
            _context.Set<DatabaseSource>().Remove(dbSource);
            await _context.SaveChangesAsync(ct);

            RemoveFromDbSchemaStorage(databaseSourceId);
        }
    }

    public async Task<DbSchema> ReloadDatabaseSource(Guid databaseSourceId, CancellationToken ct)
    {
        CheckStorageInitialized();

        var dbSource = await _context.Set<DatabaseSource>().SingleOrDefaultAsync(x => x.Id == databaseSourceId, ct);
        var dbSchema = CreateDbSchemaFromDbSource(dbSource)
            ?? throw new DataException($"Error creating DB schema from DB source with ID {databaseSourceId}");

        RemoveFromDbSchemaStorage(databaseSourceId);
        AddToDbSchemaStorage(dbSchema);

        return dbSchema;
    }

    public async Task<bool> IsSchemaCodeUnique(string schemaCode, CancellationToken ct) =>
        !await _context.Set<DatabaseSource>().AnyAsync(x => x.SchemaCode == schemaCode, ct);

    private IList<DbSchema> CreateDbSchemasFromDbSources()
    {
        var dbSchemas = new List<DbSchema>();

        var databaseSources = _context.Set<DatabaseSource>().ToList();

        foreach (var dbSource in databaseSources)
        {
            var dbSchema = CreateDbSchemaFromDbSource(dbSource);

            if (dbSchema != null)
            {
                dbSchemas.Add(dbSchema);
            }
        }

        return dbSchemas;
    }

    private DbSchema CreateDbSchemaFromDbSource(DatabaseSource dbSource)
    {
        DbSchema dbSchema = null;

        try
        {
            var registerRequest = MapDbSourceToRegisterRequest(dbSource);

            dbSchema = ReadDbSchemaFromDbSource(registerRequest);
            if (dbSchema != null)
            {
                dbSchema.DatabaseSource = GetDecryptedDbSource(dbSource);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"DB Documenting: Error loading DB schema for database source " +
                $"(ID: {dbSource.Id}, code: {dbSource.SchemaCode})");
        }

        return dbSchema;
    }

    private DbSchema ReadDbSchemaFromDbSource(DatabaseSourceRegisterRequest registerRequest)
    {
        var schemaReader = new ConnectedDbSchemaReader(registerRequest.ConnectionString, registerRequest.DatabaseType);
        return schemaReader.ReadSchema(registerRequest.SchemaCode, this, default).Result;
    }

    private static DatabaseSourceRegisterRequest MapDbSourceToRegisterRequest(DatabaseSource source) =>
        new()
        {
            ContextId = source.ContextId,
            SchemaCode = source.SchemaCode,
            DatabaseType = source.DatabaseType,
            ConnectionString = source.ConnectionString?.AesDecryptBase64(sensitiveDbSourceDataEncryptKey)
        };

    private static DatabaseSource MapRegisterRequestToDbSource(DatabaseSourceRegisterRequest registerRequest) =>
        new()
        {
            ContextId = registerRequest.ContextId,
            SchemaCode = registerRequest.SchemaCode,
            DatabaseType = registerRequest.DatabaseType,
            ConnectionString = registerRequest.ConnectionString?.AesEncryptBase64(sensitiveDbSourceDataEncryptKey)
        };

    private static DatabaseSource GetDecryptedDbSource(DatabaseSource databaseSource)
    {
        var ds = databaseSource.DeepCopy();
        ds.ConnectionString = ds.ConnectionString?.AesDecryptBase64(sensitiveDbSourceDataEncryptKey);
        return ds;
    }

    //TODO: consider explicit initialization once and then throw exception in methods if storage not initialized
    private void CheckStorageInitialized()
    {
        // Outer check to avoid bottleneck caused by locking on many requests
        if (!_isDbSchemaStorageInitialized)
        {
            lock (_dbSchemaStorageLock)
            {
                // Inner check to avoid unlocked thread duplicate creating DB schemas 
                if (!_isDbSchemaStorageInitialized)
                {
                    _dbSchemaStorage = CreateDbSchemasFromDbSources();
                    _isDbSchemaStorageInitialized = true;
                }

            }
        }
    }

    private IList<DbSchema> GetDbSchemaStorage()
    {
        CheckStorageInitialized();
        return _dbSchemaStorage;
    }

    private void AddToDbSchemaStorage(DbSchema schema)
    {
        var storage = GetDbSchemaStorage();

        lock (_dbSchemaStorageLock)
        {
            if (schema != null)
            {
                storage?.Add(schema);
            }
        }
    }

    private void RemoveFromDbSchemaStorage(Guid databaseSourceId)
    {
        var storage = GetDbSchemaStorage();

        lock (_dbSchemaStorageLock)
        {
            var schema = storage?.FirstOrDefault(x => x.DatabaseSource.Id == databaseSourceId);
            if (schema != null)
            {
                storage?.Remove(schema);
            }
        }
    }
}
