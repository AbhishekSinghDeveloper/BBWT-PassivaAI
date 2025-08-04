using BBWM.FormIO.Classes;

namespace BBWM.FormIO.Interfaces.FormVersioningInterfaces;

public interface IFormDataVersioningService
{
    void UpdateFormDataInBackground(int definitionId, IEnumerable<FormFieldDataUpdate> updates);
    Task UpdateFormData(int definitionId, IEnumerable<FormFieldDataUpdate> formDataUpdates, CancellationToken ct = default);
}