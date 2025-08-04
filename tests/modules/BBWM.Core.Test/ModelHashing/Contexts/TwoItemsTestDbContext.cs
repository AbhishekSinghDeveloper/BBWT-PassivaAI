using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.DTO;

using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

using Z.EntityFramework.Plus;

namespace BBWT.Tests.modules.BBWM.Core.Test.ModelHashing.Contexts;

public class TwoItemsTestDbContext : DbContext, IDbContext, IDisposable
{
    protected TwoItemsTestDbContext(DbContextOptions options) : base(options)
    {
    }

    public static TwoItemsTestDbContext CreateForInMemory()
    {
        var options = new DbContextOptionsBuilder<TwoItemsTestDbContext>()
           .UseInMemoryDatabase(Guid.NewGuid().ToString())
           .Options;

        return new TwoItemsTestDbContext(options);
    }

    public static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MasterMappingProfile>();
            cfg.AddProfile<DetailMappingProfile>();
        });
        return configuration.CreateMapper();
    }

    public DbSet<Master> Masters { get; set; }

    public DbSet<Detail> Details { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
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

public class Master
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }

    public List<Detail> Details { get; set; } = new List<Detail>();
}

public class Detail
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int MasterId { get; set; }

    public virtual Master Master { get; set; }
}

public class MasterDTO : IDTO
{
    public int Id { get; set; }

    public string Name { get; set; }

    public List<DetailDTO> Details { get; set; } = new List<DetailDTO>();
}

public class DetailDTO : IDTO
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int MasterId { get; set; }

    public virtual MasterDTO Master { get; set; }
}

public class MasterMappingProfile : Profile
{
    public MasterMappingProfile()
    {
        CreateMap<Master, MasterDTO>().ReverseMap();
    }
}

public class DetailMappingProfile : Profile
{
    public DetailMappingProfile()
    {
        CreateMap<Detail, DetailDTO>().ReverseMap();
    }
}
