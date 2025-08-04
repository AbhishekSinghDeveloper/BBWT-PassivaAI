using BBWM.Core.Data;
using BBWM.Core.DTO;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Reflection;

using Z.EntityFramework.Plus;

namespace BBWM.Core.Test.Contexts;

public class TwoItemsTestDbContext : DbContext, IDbContext, IDisposable
{
    protected TwoItemsTestDbContext(DbContextOptions options) : base(options)
    {
    }

    public static TwoItemsTestDbContext CreateForInMemory()
    {
        var options = new DbContextOptionsBuilder<TwoItemsTestDbContext>()
           .UseInMemoryDatabase(ConnectionStringHelper.GetInMemoryConnectionString())
           .Options;

        return new TwoItemsTestDbContext(options);
    }

    public static TwoItemsTestDbContext CreateForSqlLite()
    {
        DbConnection _connection = new SqliteConnection(ConnectionStringHelper.GetSqlLiteConnectionString());
        _connection.Open();

        var options = new DbContextOptionsBuilder<TwoItemsTestDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new TwoItemsTestDbContext(options);
    }

    public DbSet<Master> Masters { get; set; }

    public DbSet<Detail> Details { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    internal static IDbContext CreateForMySql()
    {
        var connectionString = ConnectionStringHelper.GetMySqlConnectionString();
        var options = new DbContextOptionsBuilder<TwoItemsTestDbContext>()
            .UseMySql(connectionString, ServerVersion.Parse("8.0"))
            .Options;

        return new TwoItemsTestDbContext(options);
    }

    internal static IDbContext CreateForSqlServer()
    {
        var options = new DbContextOptionsBuilder<TwoItemsTestDbContext>()
            .UseSqlServer(ConnectionStringHelper.GetSqlServerConnectionString())
            .Options;

        return new TwoItemsTestDbContext(options);
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

public class Master : IEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(35)]
    public string Name { get; set; }

    public List<Detail> Details { get; set; } = new List<Detail>();
}

public class Detail : IEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(35)]
    public string Name { get; set; }

    public int MasterId { get; set; }

    public virtual Master Master { get; set; }
}

public class MasterDto : IDTO
{
    public int Id { get; set; }

    public string Name { get; set; }

    public List<DetailDto> Details { get; set; } = new List<DetailDto>();
}

public class DetailDto : IDTO
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int MasterId { get; set; }

    public virtual MasterDto Master { get; set; }
}

public class SimpleDetailDto : IDTO
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string MasterName { get; set; }
}

public class ExtendedDetailDto : DetailDto
{
    public string MasterName { get; set; }
}

public class MasterMappingProfile : AutoMapper.Profile
{
    public MasterMappingProfile()
    {
        CreateMap<Master, MasterDto>().ReverseMap();
    }
}

public class DetailMappingProfile : AutoMapper.Profile
{
    public DetailMappingProfile()
    {
        CreateMap<Detail, DetailDto>()
            .ReverseMap();

        CreateMap<Detail, SimpleDetailDto>()
            .ForMember(e => e.MasterName, r => r.MapFrom(x => x.Master.Name))
            .ReverseMap();

        CreateMap<Detail, ExtendedDetailDto>()
            .ForMember(e => e.MasterName, r => r.MapFrom(x => x.Master.Name))
            .ReverseMap();
    }
}
