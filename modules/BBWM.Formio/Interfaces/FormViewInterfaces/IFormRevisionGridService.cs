using BBWM.FormIO.Classes;

namespace BBWM.FormIO.Interfaces.FormViewInterfaces;

public interface IFormRevisionGridService
{
    Task<FormRevisionGridUpdate> UpdateRevisionGrids(int definitionId, CancellationToken ct = default);
    Task<FormRevisionGridUpdate> UpdateRevisionGrids(int definitionId, string viewName, FormField root, CancellationToken ct = default);
}