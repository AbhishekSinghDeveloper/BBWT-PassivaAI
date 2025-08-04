using BBWM.Core.Exceptions;
using BBWM.Reporting.Interfaces;

namespace BBWM.Reporting.Services;

public class QuerableTableSourceService : IQueryableTableSourceService
{
    private readonly IServiceProvider serviceProvider;

    public QuerableTableSourceService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <summary>
    /// In this version it retrieves only DB view type of queryable table source.
    /// In the next versions can be extended with other sources types (like JSON-based, XLS tables)
    /// </summary>
    public async Task<IEnumerable<QueryableTableSource>> GetQueryableTableSources(CancellationToken ct)
    {
        var sources = new List<QueryableTableSource>();

        foreach (var provider in TableProviders)
        {
            var source = await provider.GetQueryableTableSource(ct);
            if (source != null)
                sources.Add(source);
        }

        return sources;
    }

    public async Task<QueryableTableSource> GetQueryableTableSource(string sourceCode, CancellationToken ct)
    {
        var tableProvider = TableProviders.FirstOrDefault(x => x.SourceCode == sourceCode)
            ?? throw new ObjectNotExistsException($"Queryable table provider with code '{sourceCode}' not found ");

        return await tableProvider.GetQueryableTableSource(ct);
    }

    private IEnumerable<IQueryableTableProvider> TableProviders =>
        QueryableTablesProvidersRegister.GetQueryableTablesProviders(serviceProvider);

}