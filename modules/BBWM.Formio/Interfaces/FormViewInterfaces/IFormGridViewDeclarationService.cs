using BBWM.FormIO.Classes;

namespace BBWM.FormIO.Interfaces.FormViewInterfaces;

public interface IFormGridViewDeclarationService
{
    IList<FormViewColumnItem> GetFormGridViewColumns(string viewName, FormField root);
    Task CreateFormRevisionGridView(int revisionId, CancellationToken ct = default);
    Task CreateFormRevisionGridView(int revisionId, string viewName, FormField root, CancellationToken ct = default);
}