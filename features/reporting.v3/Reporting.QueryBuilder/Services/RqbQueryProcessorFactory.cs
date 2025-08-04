using BBF.Reporting.QueryBuilder.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;

namespace BBF.Reporting.QueryBuilder.Services;

public class RqbQueryProcessorFactory : IRqbQueryProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public RqbQueryProcessorFactory(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    private static readonly Dictionary<DatabaseType, Type> SqlQueryProviderTypesDic = new();

    public void RegisterSqlQueryProvider<T>(DatabaseType type)
    {
        var pType = typeof(T);
        if (_serviceProvider.GetService(pType) is null)
            throw new ObjectNotExistsException($"Provider's interface '{pType.Name}' not found");
        SqlQueryProviderTypesDic.Add(type, pType);
    }

    /// <summary>
    /// Creates new class instance of <see cref="IRqbQueryProcessor"/> , determined by type
    /// (<see cref="DatabaseType"/>).
    /// </summary>
    public IRqbQueryProcessor? GetSqlQueryProvider(DatabaseType type)
        => SqlQueryProviderTypesDic.TryGetValue(type, out var serviceType)
            ? _serviceProvider.GetService(serviceType) as IRqbQueryProcessor
            : null;
}