namespace BBWM.Reporting.Interfaces;

/// <summary>
/// Manages access to all querable tables collected from providers registered in the reporting
/// </summary>
public interface IQueryableTableSourceService
{
    public Task<IEnumerable<QueryableTableSource>> GetQueryableTableSources(CancellationToken ct);
    public Task<QueryableTableSource> GetQueryableTableSource(string sourceCode, CancellationToken ct);
}