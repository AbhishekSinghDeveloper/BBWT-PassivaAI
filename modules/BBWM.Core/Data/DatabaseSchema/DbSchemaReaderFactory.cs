using BBWM.Core.Data.DatabaseSchema.ReaderMsSql;
using BBWM.Core.Data.DatabaseSchema.ReaderMySql;
using BBWM.Core.Data.DatabaseSchema.ReaderPostgreSql;

namespace BBWM.Core.Data.DatabaseSchema;

public static class DbSchemaReaderFactory
{
    public static IDbSchemaReader CreateDbSchemaReader(DatabaseType dbType) =>
        dbType switch
        {
            DatabaseType.MySql => new MySqlDbSchemaReader(),
            DatabaseType.MsSql => new MsSqlDbSchemaReader(),
            DatabaseType.PostgreSql => new PostgreSqlDbSchemaReader(),
            _ => throw new NotImplementedException()
        };
}