using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Data.DatabaseSchema.ReaderMySql;

/// <summary>
/// </summary>
public class MySqlDbSchemaContext : DbContext
{
    public MySqlDbSchemaContext(DbContextOptions<MySqlDbSchemaContext> options) : base(options)
    {
    }

    public string DbSchemaName => Database.GetDbConnection().Database;

    public IQueryable<MySqlInfoSchemaTable> InfoSchemaTables =>
        Tables.FromSqlRaw($"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='{DbSchemaName}'");

    public IQueryable<MySqlInfoSchemaColumn> InfoSchemaColumns =>
        Columns.FromSqlRaw($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='{DbSchemaName}'");

    public IQueryable<MySqlInfoSchemaKeyColumnUsage> InfoSchemaKeyColumnUsage =>
        KeyColumnUsage.FromSqlRaw($"SELECT * FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE REFERENCED_TABLE_SCHEMA='{DbSchemaName}'");

    private DbSet<MySqlInfoSchemaTable> Tables { get; set; }

    private DbSet<MySqlInfoSchemaColumn> Columns { get; set; }

    private DbSet<MySqlInfoSchemaKeyColumnUsage> KeyColumnUsage { get; set; }

}