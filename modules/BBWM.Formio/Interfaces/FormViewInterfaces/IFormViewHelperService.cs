using BBWM.FormIO.Classes;

namespace BBWM.FormIO.Interfaces.FormViewInterfaces;

public interface IFormViewHelperService
{
    IEnumerable<FormViewJsonTableColumn> GetJsonTableColumns(FormField root);
    IEnumerable<FormViewColumnItem> GetViewColumns(string viewName, FormField root);
    Task<string> GetFormUniqueViewName(string? name, CancellationToken ct = default);
    Task CreateView(string query, CancellationToken ct = default);
    Task DeleteView(string viewName, CancellationToken ct = default);
}