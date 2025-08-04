using BBWM.Core.Filters.TypedFilters;

namespace BBWM.Core.Filters;

public class QueryFilter<TEntity>
{
    public IList<FilterInfoBase> Filters { get; }
    public IQueryable<TEntity> Query { get; set; }

    public QueryFilter(IList<FilterInfoBase> filters, IQueryable<TEntity> query)
    {
        Filters = filters;
        Query = query;
    }

    public QueryFilter<TEntity> Handle<T>(string filterName, Func<IQueryable<TEntity>, T, IQueryable<TEntity>> filterHandler)
        where T : class
    {
        var filterInfo = Filters.FirstOrDefault(x =>
            x.GetType() == typeof(T) &&
            x.PropertyName.Equals(filterName, StringComparison.InvariantCultureIgnoreCase));

        if (filterInfo is not null)
        {
            Query = filterHandler(Query, filterInfo as T);
            Filters.Remove(filterInfo);
        }

        return this;
    }

    public QueryFilter<TEntity> Handle(string filterName, Func<IQueryable<TEntity>, StringFilter, IQueryable<TEntity>> filterHandler)
        => Handle<StringFilter>(filterName, filterHandler);

    public QueryFilter<TEntity> SetQuery(IQueryable<TEntity> query)
    {
        Query = query;
        return this;
    }
}
