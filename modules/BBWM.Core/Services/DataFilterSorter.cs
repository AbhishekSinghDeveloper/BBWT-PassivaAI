using BBWM.Core.Extensions;
using BBWM.Core.Filters;
using BBWM.Core.Filters.Handlers;

using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace BBWM.Core.Services;

public static class DataFilterSorter<TEntity>
    where TEntity : class
{
    public static IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query, Filter filter)
    {
        if (filter.Filters is null || !filter.Filters.Any()) return query;

        foreach (var groupedFilters in filter.Filters.GroupBy(a => a.PropertyName.ToLowerInvariant()))
        {
            Expression<Func<TEntity, bool>> orExpression = null;
            foreach (var filterInfo in groupedFilters)
            {
                CheckPropertyChain(filterInfo.PropertyName);
                var expr = FilterHandlersProvider.ProvideFilter<TEntity>(filterInfo);
                if (expr is not null)
                {
                    orExpression = orExpression is null ? expr : orExpression.Or(expr);
                }
            }

            if (orExpression is not null)
            {
                query = query.Where(orExpression);
            }
        }

        return query;
    }

    public static IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, ISorter command)
    {
        if (string.IsNullOrEmpty(command.SortingField)) return query;

        // TODO: it can be a better place to decode it - tthe same as for FilterInfoModelBinder - in model filters
        command.SortingField = WebUtility.UrlDecode(command.SortingField);

        if (command.SortingField.EndsWith("_original"))
            command.SortingField = command.SortingField.Remove(command.SortingField.Length - 9);

        CheckPropertyChain(command.SortingField);
        var sortingDirection = command.SortingDirection ?? OrderDirection.Asc;

        var param = Expression.Parameter(typeof(TEntity), "item");
        var body = command.SortingField.Split('.').Aggregate<string, Expression>(param, Expression.Property);
        var lambda = Expression.Lambda(body, param);

        var method = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(a => a.Name == $"OrderBy{(sortingDirection == OrderDirection.Asc ? string.Empty : "Descending")}")
            .Single(a => a.GetParameters().Length == 2);
        method = method.MakeGenericMethod(typeof(TEntity), body.Type);
        return (IQueryable<TEntity>)method.Invoke(method, new object[] { query, lambda });
    }

    private static void CheckPropertyChain(string propertyChain)
    {
        var propertyName = propertyChain?.Split('.') ?? new string[0];
        PropertyInfo entityPropertyInfo = null;
        var type = typeof(TEntity);

        foreach (var part in propertyName)
        {
            entityPropertyInfo = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (entityPropertyInfo is not null)
            {
                type = entityPropertyInfo.PropertyType;
            }
            else
            {
                break;
            }
        }

        if (entityPropertyInfo is null)
        {
            throw new ArgumentException($"Can not find '{propertyChain}' property " + $"in '{typeof(TEntity).FullName}' entity type");
        }
    }
}
