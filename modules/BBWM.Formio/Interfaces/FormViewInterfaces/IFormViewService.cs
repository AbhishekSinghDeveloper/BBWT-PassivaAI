namespace BBWM.FormIO.Interfaces.FormViewInterfaces;

public interface IFormViewService
{
    Task UpdateViewRelatedData(int definitionId, CancellationToken ct = default);
    Task DeleteViewRelatedData(int definitionId, CancellationToken ct = default);
}