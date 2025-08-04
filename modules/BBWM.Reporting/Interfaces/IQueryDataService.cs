using BBWM.Core.Filters;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Model;

namespace BBWM.Reporting.Interfaces;

public interface IQueryDataService
{
    Task<IEnumerable<dynamic>> GetData(Query query, QueryCommand queryCommand = null, CancellationToken ct = default);
    Task<IEnumerable<DropDownOption>> GetDataAsOptions(QueryBuilderOptionsRequest request, Query query, CancellationToken ct = default);
    Task<string> GetSqlQuery(Query query, bool reduceSyntax = false, CancellationToken ct = default);
    Task<int> GetTotal(Query query, QueryCommand queryCommand = null, CancellationToken ct = default);
    Task<dynamic> GetAggregations(
        Query query,
        IEnumerable<dynamic> aggregations,
        QueryCommand queryCommand = null,
        CancellationToken ct = default);
}
