using BBWM.DbDoc.DbSchemas.SchemaModels;

namespace BBWM.DbDoc.DbSchemas.Interfaces;

public interface IDatabaseSchemaReader
{
    Task<DbSchema> ReadSchema(string schemaCode, IDbSchemaCodeValidator schemaCodeValidator, CancellationToken ct = default);
}
