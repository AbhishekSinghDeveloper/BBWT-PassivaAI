namespace BBWM.DbDoc.DbSchemas.Interfaces;

public interface IDbSchemaCodeValidator
{
    Task<bool> IsSchemaCodeUnique(string schemaCode, CancellationToken ct);
}
