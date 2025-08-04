using BBWM.FormIO.Classes;

namespace BBWM.FormIO.Interfaces.FormViewInterfaces;

public interface IFormViewDeclarationService
{
    IList<FormViewColumnItem> GetFormViewColumns(string viewName, FormField root);
    Task CreateFormRevisionView(int revisionId, CancellationToken ct = default);
    Task CreateFormRevisionView(int revisionId, string viewName, FormField root, CancellationToken ct = default);
}