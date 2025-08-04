using System.Linq.Expressions;
using System.Reflection;

namespace BBWM.Core.Filters.Handlers;

public static class FilterHandlersProvider
{
    public static Expression<Func<TEntity, bool>> ProvideFilter<TEntity>(FilterInfoBase filter)
    {
        try
        {
            var attribute = filter.GetType().GetTypeInfo().GetCustomAttribute<RelatedHandlerAttribute>();
            var handler = Activator.CreateInstance(attribute.HandlerType, filter) as FilterHandlerBase;
            return handler?.Handle<TEntity>();
        }
        catch
        {
            // should log exception here
            return null;
        }
    }
}
