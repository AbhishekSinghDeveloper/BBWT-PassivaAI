using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;

namespace BBF.Reporting.Core.Services;

public class QueryProviderFactory : IQueryProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContext _context;

    public QueryProviderFactory(IServiceProvider serviceProvider, IDbContext context)
    {
        _serviceProvider = serviceProvider;
        _context = context;
    }

    #region Query Source provider

    private static readonly Dictionary<string, Type> QuerySourceProviderTypesDic = new();

    public void RegisterQueryProvider<T>(string code)
    {
        var pType = typeof(T);
        if (_serviceProvider.GetService(pType) is null)
            throw new ObjectNotExistsException($"Provider's interface '{pType.Name}' not found");
        QuerySourceProviderTypesDic.Add(code, pType);
    }

    public IQuerySourceProvider? GetQueryProvider(string code) =>
        QuerySourceProviderTypesDic.TryGetValue(code, out var serviceType)
            ? _serviceProvider.GetService(serviceType) as IQuerySourceProvider
            : null;

    public IQuerySourceProvider? GetQueryProvider(Guid querySourceId)
    {
        var querySourceType = _context.Set<QuerySource>()
            .Where(x => x.Id == querySourceId)
            .Select(x => x.QueryType)
            .First();

        return GetQueryProvider(querySourceType);
    }

    public IEnumerable<IQuerySourceProvider?> GetQueryProviders()
        => QuerySourceProviderTypesDic.Keys.Select(GetQueryProvider);

    #endregion

    #region View Metadata provider

    private static readonly Dictionary<string, Type> ViewMetadataProviderTypesDic = new();

    public void RegisterMetadataProvider<T>(string code)
    {
        var pType = typeof(T);
        if (_serviceProvider.GetService(pType) is null)
            throw new ObjectNotExistsException($"Provider's interface '{pType.Name}' not found");
        ViewMetadataProviderTypesDic.Add(code, pType);
    }

    public IViewMetadataProvider? GetMetadataProvider(Guid querySourceId)
    {
        var querySourceType = _context.Set<QuerySource>()
            .Where(x => x.Id == querySourceId)
            .Select(x => x.QueryType)
            .First();

        return GetMetadataProvider(querySourceType);
    }

    public IViewMetadataProvider? GetMetadataProvider(string code) =>
        ViewMetadataProviderTypesDic.TryGetValue(code, out var serviceType)
            ? _serviceProvider.GetService(serviceType) as IViewMetadataProvider
            : null;

    public IEnumerable<IViewMetadataProvider?> GetMetadataProviders()
        => ViewMetadataProviderTypesDic.Keys.Select(GetMetadataProvider);

    #endregion
}