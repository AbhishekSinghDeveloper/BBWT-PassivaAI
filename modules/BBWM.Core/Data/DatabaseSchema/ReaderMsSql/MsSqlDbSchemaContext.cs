using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Data.DatabaseSchema.ReaderMsSql;

public class MsSqlDbSchemaContext : DbContext
{
    public MsSqlDbSchemaContext(DbContextOptions<MsSqlDbSchemaContext> options) : base(options)
    {
    }

    public string DbSchemaName => Database.GetDbConnection().Database;

    public IQueryable<MsSqlInfoSchemaTable> InfoSchemaTables =>
        Tables.FromSqlRaw($"SELECT * FROM [{DbSchemaName}].INFORMATION_SCHEMA.TABLES");

    public IQueryable<MsSqlInfoSchemaTableConstraints> InfoSchemaTableConstraints =>
        TableConstraints.FromSqlRaw($"SELECT * FROM [{DbSchemaName}].INFORMATION_SCHEMA.TABLE_CONSTRAINTS");

    public IQueryable<MsSqlInfoSchemaKeyColumnUsage> InfoSchemaKeyColumnUsage =>
        KeyColumnUsage.FromSqlRaw($"SELECT * FROM [{DbSchemaName}].INFORMATION_SCHEMA.KEY_COLUMN_USAGE");

    public IQueryable<MsSqlInfoSchemaColumn> InfoSchemaColumns =>
        Columns.FromSqlRaw($"SELECT * FROM [{DbSchemaName}].INFORMATION_SCHEMA.COLUMNS");

    public IQueryable<MsSqlInfoSchemaReferentialConstraints> InfoSchemaReferentialConstraints =>
        ReferentialConstraints.FromSqlRaw($"SELECT * FROM [{DbSchemaName}].INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS");

    private DbSet<MsSqlInfoSchemaTable> Tables { get; set; }
    private DbSet<MsSqlInfoSchemaTableConstraints> TableConstraints { get; set; }
    private DbSet<MsSqlInfoSchemaKeyColumnUsage> KeyColumnUsage { get; set; }
    private DbSet<MsSqlInfoSchemaColumn> Columns { get; set; }
    private DbSet<MsSqlInfoSchemaReferentialConstraints> ReferentialConstraints { get; set; }

}