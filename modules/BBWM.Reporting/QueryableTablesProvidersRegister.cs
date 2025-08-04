using BBWM.Reporting.Interfaces;
using Castle.DynamicProxy.Internal;

namespace BBWM.Reporting;

public static class QueryableTablesProvidersRegister
{
    private static readonly IList<Type> tablesProviders = new List<Type>();

    public static IEnumerable<IQueryableTableProvider> GetQueryableTablesProviders(IServiceProvider serviceProvider) =>
        tablesProviders
        .Where(x => x is not null)
        .Select(x => (IQueryableTableProvider)serviceProvider.GetService(x))
        .Where(x => x is not null)
        .ToList();

    /// <summary>
    /// Adds quaryable tables (or views) provider to the list of registered providers of the reporting.
    /// 
    /// Features (like Forms) can register their internal provider which converts JSON-stored tables data
    /// into quaryable tables wrapped into DB views. Then in the report builder you can build a query based
    /// on these "virtual" tables and make a report for the end-user showing records from submitted forms data.
    /// </summary>
    public static void RegisterQueryableTablesProvider(Type type)
    {
        if (type is null) return;

        if (type.GetAllInterfaces().All(i => i != typeof(IQueryableTableProvider)))
            throw new ApplicationException($"Type '{type.Name}' should implement IQueryableTableProvider.");

        tablesProviders.Add(type);

    }
}
