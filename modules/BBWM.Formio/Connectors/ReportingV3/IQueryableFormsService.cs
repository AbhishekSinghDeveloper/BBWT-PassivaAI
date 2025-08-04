namespace BBWM.FormIO.Connectors.ReportingV3;

public interface IQueryableFormsService
{
    Task<IEnumerable<QueryableForm>> GetQueryableForms(bool includeColumns, bool includeChildren, CancellationToken ct = default);
    Task<QueryableForm?> GetQueryableForm(string formId, string? parentFormId, CancellationToken ct = default);
}