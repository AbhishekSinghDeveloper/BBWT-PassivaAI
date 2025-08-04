using AutoMapper;
using AutoMapper.QueryableExtensions;

using BBWM.Core.Data;
using BBWM.Core.DTO;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Services;

/// <summary>
/// Default Data Service.
/// </summary>
/// <remarks>
/// <para>
/// The service aims for these purposes:
/// </para>
/// <list type="number">
/// <item>It’s a default implementation for CRUD operations required by the base controller
/// <c>DataControllerBase</c>. The controller performs default operations with database context,
/// unless the controller's behavior is customized by adding a handler class into the constructor.
/// </item>
/// <item>It’s very handy to be used on the API controller’s level to run specific methods,
///     for example, to get a page with specific model/DTO types, to get a model’s DTO record etc,
///     to Get All records, delete All records, everything that data operation allows.
///     It encapsulates work with context and mapper. As a key result – a single line call
///     replaces involving of business layer and adding interface’s extra methods. Note,
///     we don’t allow GetAll() and DeleteAll() in API by default due to security questions,
///     and that’s when the DataService becomes extremely handy, allowing to get all records
///     mapped to DTOs with a single code line.
///     With the same success, it can be used in the services layer.
/// </item>
/// </list>
/// <para>It only works with a database context (inherited form <see cref="IDbContext"/>) because the data service is designed
/// to serve the only purpose to manage database entities.</para>
/// <para>Also it takes responsibility for entities mapping. It follows from the idea that the service
/// only gets DTOs as input and only returns DTOs as output.</para>
/// <para>As the service implementing this interface is responsible for incapsulating work with database context and mapper,
/// it becomes a handy tool for developer in cases where it's easier (and acceptable) to avoid direct usage of context and mapping.
/// </para>
/// <para>
/// Nevertheless, the service should be used carefully, avoiding misusing in cases when direct work with the context is preferable.
/// For example, when a business service's method performs a complex set of operations with data entities which should be finally saved
/// just once, it's better to use the context object and avoid intermediate saving.
/// Because data service (IDataService) fully isolates work with context and mapping and saves data to database on each call of write method.
/// </para>
/// <para><see href="https://wiki.bbconsult.co.uk/display/BLUEB/Grid+Pages+Automation">See Wiki page</see></para>
/// </remarks>
/// <typeparam name="TContext">Database context type.</typeparam>
public class DataService<TContext> : IDataService<TContext>
    where TContext : IDbContext
{
    private readonly TContext _context;
    private readonly IMapper _mapper;

    public TContext Context => _context;
    public IMapper Mapper => _mapper;

    public DataService(TContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TDTO> Create<TEntity, TDTO>(TDTO dto, CancellationToken ct)
        where TEntity : class
        where TDTO : class
    {
        var entity = _mapper.Map<TEntity>(dto);
        await _context.Set<TEntity>().AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<TDTO>(entity);
    }

    public async Task<TDTO> Create<TEntity, TDTO>(TDTO dto, Action<TEntity, TContext> beforeSave, CancellationToken ct)
        where TEntity : class
        where TDTO : class
    {
        var entity = _mapper.Map<TEntity>(dto);
        await _context.Set<TEntity>().AddAsync(entity, ct);

        beforeSave?.Invoke(entity, _context);

        await _context.SaveChangesAsync(ct);
        return _mapper.Map<TDTO>(entity);
    }

    public async Task<TDTO> Update<TEntity, TDTO, TKey>(TDTO dto, CancellationToken ct)
        where TKey : IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
        where TDTO : class, IDTO<TKey>
    {
        var entity = await _context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id.Equals(dto.Id), ct);
        if (entity is null)
            throw new EntityNotFoundException();

        _mapper.Map(dto, entity);
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<TDTO>(entity);
    }

    public async Task<TDTO> Update<TEntity, TDTO, TKey>(TDTO dto, Action<TEntity, TContext> beforeSave, CancellationToken ct)
        where TKey : IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
        where TDTO : class, IDTO<TKey>
    {
        var entity = await _context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id.Equals(dto.Id), ct);
        if (entity is null)
            throw new EntityNotFoundException();

        _mapper.Map(dto, entity);

        beforeSave?.Invoke(entity, _context);

        await _context.SaveChangesAsync(ct);
        return _mapper.Map<TDTO>(entity);
    }

    public Task<TDTO> Update<TEntity, TDTO>(TDTO dto, CancellationToken ct)
        where TEntity : class, IEntity
        where TDTO : class, IDTO
        => Update<TEntity, TDTO, int>(dto, ct);

    public Task<TDTO> Update<TEntity, TDTO>(TDTO dto, Action<TEntity, TContext> beforeSave, CancellationToken ct)
        where TEntity : class, IEntity
        where TDTO : class, IDTO
        => Update<TEntity, TDTO, int>(dto, beforeSave, ct);

    public async Task<TDTO> Get<TEntity, TDTO, TKey>(TKey id, IEntityQuery<TEntity> queryHandler, CancellationToken ct = default)
        where TKey : IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
        where TDTO : class, IDTO<TKey>
    {
        var query = queryHandler?.GetEntityQuery(_context.Set<TEntity>()) ?? _context.Set<TEntity>();
        return _mapper.Map<TDTO>(await query.AsNoTracking().FirstOrDefaultAsync(o => o.Id.Equals(id), ct));
    }

    public async Task<TDTO> Get<TEntity, TDTO, TKey>(TKey id, CancellationToken ct = default)
        where TKey : IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
        where TDTO : class, IDTO<TKey>
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        var entity = await query.AsNoTracking().FirstOrDefaultAsync(o => o.Id.Equals(id), ct);

        return entity != null ? _mapper.Map<TDTO>(entity) : null;
    }

    public async Task<TDTO> Get<TEntity, TDTO, TKey>(TKey id, Func<IQueryable<TEntity>, IQueryable<TEntity>> setQuery, CancellationToken ct = default)
        where TKey : IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
        where TDTO : class, IDTO<TKey>
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        if (setQuery != null)
        {
            query = setQuery(query);
        }

        var entity = await query.AsNoTracking().FirstOrDefaultAsync(o => o.Id.Equals(id), ct);

        return entity != null ? _mapper.Map<TDTO>(entity) : null;
    }

    public async Task<TDTO> Get<TEntity, TDTO>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> setQuery, CancellationToken ct = default)
        where TEntity : class
        where TDTO : class
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        if (setQuery != null)
        {
            query = setQuery(query);
        }

        var entity = await query.AsNoTracking().FirstOrDefaultAsync(ct);

        return entity != null ? _mapper.Map<TDTO>(entity) : null;
    }

    public Task<TDTO> Get<TEntity, TDTO>(int id, IEntityQuery<TEntity> queryHandler, CancellationToken ct = default)
        where TEntity : class, IEntity
        where TDTO : class, IDTO
        => Get<TEntity, TDTO, int>(id, queryHandler, ct);

    public Task<TDTO> Get<TEntity, TDTO>(int id, CancellationToken ct = default)
        where TEntity : class, IEntity
        where TDTO : class, IDTO
        => Get<TEntity, TDTO, int>(id, ct);

    public Task<TDTO> Get<TEntity, TDTO>(int id, Func<IQueryable<TEntity>, IQueryable<TEntity>> setQuery, CancellationToken ct = default)
        where TEntity : class, IEntity
        where TDTO : class, IDTO
        => Get<TEntity, TDTO, int>(id, setQuery, ct);

    public async Task<IEnumerable<TDTO>> GetAll<TEntity, TDTO>(CancellationToken ct = default)
        where TEntity : class
        where TDTO : class
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        var entities = await query.AsNoTracking().ToListAsync(ct);

        return _mapper.Map<IEnumerable<TDTO>>(entities);
    }

    public async Task<IEnumerable<TDTO>> GetAll<TEntity, TDTO>(Filter filter, CancellationToken ct = default)
        where TEntity : class
        where TDTO : class
    {
        var query = DataFilterSorter<TEntity>.ApplyFilter(_context.Set<TEntity>(), filter);
        return _mapper.Map<IEnumerable<TDTO>>(await query.AsNoTracking().ToListAsync(ct));
    }

    public async Task<IEnumerable<TDTO>> GetAll<TEntity, TDTO>(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> setQuery, CancellationToken ct = default)
        where TEntity : class
        where TDTO : class
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        if (setQuery != null)
        {
            query = setQuery(query);
        }

        var entities = await query.AsNoTracking().ToListAsync(ct);

        return _mapper.Map<IEnumerable<TDTO>>(entities);
    }

    public async Task Delete<TEntity, TKey>(TKey id, Action<TEntity, TContext> beforeSave, CancellationToken ct = default)
        where TKey : IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
    {
        var entity = await _context.Set<TEntity>().FirstOrDefaultAsync(o => o.Id.Equals(id), ct);
        if (entity is not null)
        {
            _context.Set<TEntity>().Remove(entity);
            beforeSave?.Invoke(entity, _context);
            await _context.SaveChangesAsync(ct);
        }
    }

    public Task Delete<TEntity, TKey>(TKey id, CancellationToken ct = default)
        where TKey : IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
        => Delete<TEntity, TKey>(id, null, ct);

    public Task Delete<TEntity>(int id, Action<TEntity, TContext> beforeSave, CancellationToken ct = default)
        where TEntity : class, IEntity
        => Delete<TEntity, int>(id, beforeSave, ct);

    public Task Delete<TEntity>(int id, CancellationToken ct = default)
        where TEntity : class, IEntity
        => Delete<TEntity, int>(id, ct);

    public async Task DeleteAll<TEntity>(CancellationToken ct = default)
        where TEntity : class
    {
        _context.Set<TEntity>().RemoveRange(_context.Set<TEntity>());
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAll<TEntity>(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> setQuery, CancellationToken ct = default)
        where TEntity : class
    {
        _context.Set<TEntity>().RemoveRange(setQuery(_context.Set<TEntity>()));
        await _context.SaveChangesAsync(ct);
    }

    public async Task<PageResult<TDTO>> GetPage<TEntity, TDTO>(
            QueryCommand command,
            IEntityQuery<TEntity> queryHandler,
            CancellationToken ct = default,
            bool mappingOnDatabase = false)
        where TEntity : class
        where TDTO : class
    {
        var query = queryHandler?.GetEntityQuery(_context.Set<TEntity>()).AsNoTracking()
            ?? _context.Set<TEntity>().AsNoTracking();

        int total;
        command ??= new();

        query = DataFilterSorter<TEntity>.ApplyFilter(query, command);
        query = DataFilterSorter<TEntity>.ApplySorting(query, command);

        total = await query.CountAsync(ct);

        if (command.Skip is not null)
            query = query.Skip(command.Skip.Value);

        if (command.Take is not null)
            query = query.Take(command.Take.Value);

        return new PageResult<TDTO>
        {
            Items = mappingOnDatabase ?
                await query.ProjectTo<TDTO>(_mapper.ConfigurationProvider).ToListAsync(ct)
                : _mapper.Map<IEnumerable<TDTO>>(await query.ToListAsync(ct)),
            Total = total
        };
    }

    public async Task<PageResult<TDTO>> GetPage<TEntity, TDTO>(
            QueryCommand command,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> setQuery = null,
            Func<QueryFilter<TEntity>, QueryFilter<TEntity>> filter = null,
            Func<IQueryable<TEntity>, ISorter, IQueryable<TEntity>> sorter = null,
            CancellationToken ct = default,
            bool mappingOnDatabase = false)
        where TEntity : class
        where TDTO : class
    {
        var query = setQuery?.Invoke(_context.Set<TEntity>()).AsNoTracking()
            ?? _context.Set<TEntity>().AsNoTracking();

        int total;
        command ??= new();

        var queryFilter = filter?.Invoke(new QueryFilter<TEntity>(command.Filters, query));
        if (queryFilter is not null)
        {
            query = queryFilter.Query;
            command.Filters = queryFilter.Filters.ToList();
        }

        query = DataFilterSorter<TEntity>.ApplyFilter(query, command);

        query = sorter?.Invoke(query, command) ?? query;
        query = DataFilterSorter<TEntity>.ApplySorting(query, command);

        total = await query.CountAsync(ct);

        if (command.Skip is not null)
            query = query.Skip(command.Skip.Value);

        if (command.Take is not null)
            query = query.Take(command.Take.Value);

        return new PageResult<TDTO>
        {
            Items = mappingOnDatabase ?
                await query.ProjectTo<TDTO>(_mapper.ConfigurationProvider).ToListAsync(ct)
                : _mapper.Map<IEnumerable<TDTO>>(await query.ToListAsync(ct)),
            Total = total
        };
    }

    public Task<PageResult<TDTO>> GetPage<TEntity, TDTO>(
            QueryCommand command,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> setQuery,
            CancellationToken ct = default,
            bool mappingOnDatabase = false)
        where TEntity : class
        where TDTO : class
        => GetPage<TEntity, TDTO>(command, setQuery, null, null, ct, mappingOnDatabase);

    public Task<PageResult<TDTO>> GetPage<TEntity, TDTO>(
            QueryCommand command,
            CancellationToken ct = default,
            bool mappingOnDatabase = false)
        where TEntity : class
        where TDTO : class
        => GetPage<TEntity, TDTO>(command, null, null, null, ct, mappingOnDatabase);

    public Task<bool> Any<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> setQuery, CancellationToken ct = default)
        where TEntity : class
        => setQuery(_context.Set<TEntity>()).AnyAsync(ct);
}

/// <summary>
/// Default Data Service.
/// Service variation for entities with integer ID field.
/// </summary>
/// <inheritdoc/>
public class DataService : DataService<IDbContext>, IDataService
{
    public DataService(IDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}
