namespace BBWM.Reporting.Interfaces;

public interface IQueryableTableProvider
{
    string SourceCode { get; }

    // TODO: likely we should pass the current user, because we should only provide tables accessible by the user
    Task<QueryableTableSource> GetQueryableTableSource(CancellationToken ct);
}
