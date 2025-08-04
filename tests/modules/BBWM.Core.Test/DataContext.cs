using BBWM.Core.Services;
using BBWM.Menu.Db;

using BBWT.Data;
using BBWT.Tests.modules.BBWM.Core.Test.Models;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using System.Data.Common;

namespace BBWM.Core.Test;

public static class InMemoryDataContext
{
    // Use the same in-memory database accross service providers
    public static readonly InMemoryDatabaseRoot InMemoryDatabaseRoot = new InMemoryDatabaseRoot();

    public static DataContext GetContext(string dbName = null)
    {
        if (string.IsNullOrEmpty(dbName))
            dbName = Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<DataContext>()
            .EnableSensitiveDataLogging()
            .UseInMemoryDatabase(dbName, InMemoryDatabaseRoot)
            .Options;

        return new DataContext(options);
    }
}

public static class SqlLiteDataContext
{
    public static DataContext GetContext()
    {
        DbConnection connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        return new DataContext(new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(connection)
            .Options);
    }
}

public class DataContext : DataContextBase, IDataContext, IMenuContext
{
    public DbSet<MenuItem> Menu { get; set; }

    public DbSet<FooterMenuItem> FooterMenuItems { get; set; }

    public DbSet<AuditableIntPKEntity> AuditableIntPKEntities { get; set; }

    public DataContext(DbContextOptions options) : base(options, new DbServices())
    {
    }
}
