
using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Services;

namespace BBWM.Core.Test.Utils;

public enum DbType
{
    InMemory,
    SqlLite
}

public class SutDataHelper
{
    public static TContext CreateEmptyContext<TContext>(DbType dbType = default, string dbName = default)
        where TContext : class =>
        dbType switch
        {
            DbType.SqlLite => SqlLiteDataContext.GetContext() as TContext,
            _ => InMemoryDataContext.GetContext(string.IsNullOrEmpty(dbName) ? Guid.NewGuid().ToString() : dbName) as TContext,
        };

    public static IDbContext CreateEmptyContext(DbType dbType = default, string dbName = default)
        => CreateEmptyContext<IDbContext>(dbType, dbName);

    public static async Task<TContext> CreateContextWithData<TContext, TEntity>(
       TEntity[] entities, DbType dbType = default, string dbName = default)
       where TContext : class, IDbContext
       where TEntity : class
    {
        var context = CreateEmptyContext<TContext>(dbType, dbName);

        await InsertData(context, entities);

        return context;
    }

    public static Task<IDbContext> CreateContextWithData<TEntity>(
        TEntity[] entities, DbType dbType = default, string dbName = default)
        where TEntity : class
        => CreateContextWithData<IDbContext, TEntity>(entities, dbType, dbName);

    public static async Task<TContext> CreateContextWithRandomData<TContext, TEntity>(
        Func<TEntity> generator, DbType dbType = default, string dbName = default, int insertNum = 3)
        where TContext : class, IDbContext
        where TEntity : class
    {
        var context = CreateEmptyContext<TContext>(dbType, dbName);
        await InsertRandomData(context, generator);
        return context;
    }

    public static Task<IDbContext> CreateContextWithRandomData<TEntity>(
        Func<TEntity> generator, DbType dbType = default, string dbName = default, int insertNum = 3)
        where TEntity : class
        => CreateContextWithRandomData<IDbContext, TEntity>(generator, dbType, dbName, insertNum);

    public static async Task InsertRandomData<TContext, TEntity>(
        TContext context, Func<TEntity> generator, int insertNum = 3)
        where TContext : class, IDbContext
        where TEntity : class
    {
        await context.Set<TEntity>().AddRangeAsync(Enumerable.Range(1, insertNum).Select(_ => generator()));
        await context.SaveChangesAsync();
    }

    public static Task InsertRandomData<TEntity>(
        IDbContext context, Func<TEntity> generator, int insertNum = 3)
        where TEntity : class
        => InsertRandomData<IDbContext, TEntity>(context, generator, insertNum);

    public static async Task InsertData<TContext, TEntity>(
        TContext context, params TEntity[] entities)
        where TEntity : class
        where TContext : IDbContext
    {
        entities ??= new TEntity[0];

        if (entities.Length > 0)
        {
            await context.Set<TEntity>().AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }
    }

    public static IDataService<TContext> CreateEmptyDataService<TContext>(
        IMapper mapper, DbType dbType = default, string dbName = default, TContext ctx = default)
            where TContext : class, IDbContext
        => new DataService<TContext>(ctx ?? CreateEmptyContext<TContext>(dbType, dbName), mapper);

    public static IDataService CreateEmptyDataService(
        IMapper mapper, DbType dbType = default, string dbName = default, IDbContext ctx = default)
        => new DataService(ctx ?? CreateEmptyContext(dbType, dbName), mapper);

    public static async Task<IDataService<TContext>> CreateDataServiceWithData<TContext, TEntity, TDto>(
        IMapper mapper,
        TDto[] entities,
        DbType dbType = default,
        string dbName = default,
        TContext ctx = default)
            where TContext : class, IDbContext
            where TEntity : class
            where TDto : class
    {
        ctx ??= CreateEmptyContext<TContext>(dbType, dbName);
        await InsertData(ctx, mapper.Map<TEntity[]>(entities ?? new TDto[0]));

        return CreateEmptyDataService(mapper, ctx: ctx);
    }

    public static async Task<IDataService> CreateDataServiceWithData<TEntity, TDto>(
        IMapper mapper,
        TDto[] entities,
        DbType dbType = default,
        string dbName = default,
        IDbContext ctx = default)
            where TEntity : class
            where TDto : class
    {
        ctx ??= CreateEmptyContext(dbType, dbName);
        await InsertData(ctx, mapper.Map<TEntity[]>(entities ?? new TDto[0]));

        return CreateEmptyDataService(mapper, ctx: ctx);
    }

    public static async Task<IDataService> CreateDataServiceWithData<TEntity>(
        IMapper mapper,
        TEntity[] entities,
        DbType dbType = default,
        string dbName = default,
        IDbContext ctx = default)
            where TEntity : class
    {
        ctx ??= CreateEmptyContext(dbType, dbName);
        await InsertData(ctx, entities);

        return CreateEmptyDataService(mapper, ctx: ctx);
    }

    public static async Task CreateRandomData<TDto>(
        IEntityCreate<TDto> sut, Func<TDto> generator, int insertNum = 3)
        where TDto : class
    {
        for (var i = 0; i < insertNum; i++)
        {
            await sut.Create(generator(), CancellationToken.None);
        }
    }
}
