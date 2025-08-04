namespace BBWM.Core.Data.DatabaseSchema.ReaderPostgreSql;

public class PostgreSqlDbSchemaReader : IDbSchemaReader
{
    public DatabaseSchema ReadDbSchema(string connectionString, int maxRetryCount = 0, int maxRetryDelay = 0)
    {
        throw new NotImplementedException();
    }
}
