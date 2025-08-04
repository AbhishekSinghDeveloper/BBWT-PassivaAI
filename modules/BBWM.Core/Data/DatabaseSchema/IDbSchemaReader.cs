namespace BBWM.Core.Data.DatabaseSchema;

public interface IDbSchemaReader
{
    public DatabaseSchema ReadDbSchema(string connectionString, int maxRetryCount = 0, int maxRetryDelay = 0);
}
