using BBWM.Core.Data;
using BBWM.Core.DTO;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Reflection;

using Z.EntityFramework.Plus;

namespace BBWM.Core.Test.Contexts;

public class OneItemTestDbContext : DbContext, IDbContext, IDisposable
{
    protected OneItemTestDbContext(DbContextOptions options) : base(options)
    {
    }

    public static OneItemTestDbContext CreateForInMemory()
    {
        var options = new DbContextOptionsBuilder<OneItemTestDbContext>()
           .UseInMemoryDatabase(ConnectionStringHelper.GetInMemoryConnectionString())
           .Options;

        return new OneItemTestDbContext(options);
    }

    public static OneItemTestDbContext CreateForSqlLite()
    {
        DbConnection _connection = new SqliteConnection(ConnectionStringHelper.GetSqlLiteConnectionString());
        _connection.Open();

        var options = new DbContextOptionsBuilder<OneItemTestDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new OneItemTestDbContext(options);
    }

    public DbSet<OneItem> OneItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    internal static IDbContext CreateForMySql()
    {
        var connectionString = ConnectionStringHelper.GetMySqlConnectionString();
        var options = new DbContextOptionsBuilder<OneItemTestDbContext>()
            .UseMySql(connectionString, ServerVersion.Parse("8.0"))
            .Options;

        return new OneItemTestDbContext(options);
    }

    internal static IDbContext CreateForSqlServer()
    {
        var options = new DbContextOptionsBuilder<OneItemTestDbContext>()
            .UseSqlServer(ConnectionStringHelper.GetSqlServerConnectionString())
            .Options;

        return new OneItemTestDbContext(options);
    }

    internal static IDbContext CreateFailedContext()
    {
        return null;
    }

    public Dictionary<PropertyInfo, Type> FindKeys(Type type)
    {
        var entityType = Model.FindEntityType(type);
        if (entityType is not null)
        {
            var foreignKeys = entityType.GetForeignKeys()
                .SelectMany(k =>
                    k.Properties.Where(p => (p.ClrType == typeof(int) || p.ClrType == typeof(int?)) && p.PropertyInfo is not null).Select(p => new
                    {
                        Property = p.PropertyInfo,
                        Principal = k.PrincipalEntityType.ClrType,
                    }))
                .ToArray();

            var primaryKeys = entityType.GetKeys()
                .SelectMany(k =>
                    k.Properties.Where(p => (p.ClrType == typeof(int) || p.ClrType == typeof(int?)) && p.PropertyInfo is not null).Select(p => new
                    {
                        Property = p.PropertyInfo,
                        Principal = type,
                    }))
                .ToArray();

            return foreignKeys.Concat(primaryKeys).GroupBy(p => p.Property).ToDictionary(g => g.Key, g => g.FirstOrDefault().Principal);
        }

        return null;
    }

    public BaseQueryFilter Filter<T>(Func<IQueryable<T>, IQueryable<T>> queryFilter, bool isEnabled = true)
    {
        return QueryFilterExtensions.Filter(this, queryFilter, isEnabled);
    }

    public BaseQueryFilter Filter<T>(object key, Func<IQueryable<T>, IQueryable<T>> queryFilter, bool isEnabled = true)
    {
        return QueryFilterExtensions.Filter(this, key, queryFilter, isEnabled);
    }
}

public class OneItem : IEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(35)]
    public string Name { get; set; }

    public DateTimeOffset? Date { get; set; }

    public int Counter { get; set; }

    public decimal? Money { get; set; }
}

public class OneItemDto : IDTO
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateTimeOffset? Date { get; set; }

    public int Counter { get; set; }

    public decimal? Money { get; set; }
}

public class OneItemMappingProfile : AutoMapper.Profile
{
    public OneItemMappingProfile()
    {   // Address
        CreateMap<OneItem, OneItemDto>().ReverseMap();
    }
}
