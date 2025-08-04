using BBWM.DbDoc.DbSchemas.DTO;
using BBWM.DbDoc.DbSchemas.SchemaModels;

namespace BBWM.DbDoc.DbSchemas.Interfaces;

public interface IDbSchemaManager
{
    /// <summary>
    /// Gets unique context ID string for DB context of main DB
    /// </summary>

    string MainDbContextId { get; }

    /// <summary>
    /// Gets database schema by DB Doc database source ID
    /// </summary>
    DbSchema GetDbSchema(Guid databaseSourceId);
    DbSchema GetDbSchema(string contextId);
    DbSchema GetMainDbSchema();
    DbSchemaTable GetTable(string tableId);
    DbSchemaColumn GetColumn(string columnId);
    IEnumerable<DbSchemaColumn> GetTableColumns(string tableId);
    DbSchema GetTableDbSchema(string tableId);    
    Task<DbSchema> RegisterNewDatabaseSource(DatabaseSourceRegisterRequest registerRequest, CancellationToken ct = default);
    Task UnregisterDatabaseSource(Guid databaseSourceId, CancellationToken ct = default);
    Task<DbSchema> ReloadDatabaseSource(Guid databaseSourceId, CancellationToken ct);
}