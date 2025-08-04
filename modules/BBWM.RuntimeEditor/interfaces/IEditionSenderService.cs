namespace BBWM.RuntimeEditor.interfaces;

public interface IEditionSenderService
{
    Task<bool> SendEditionUpdate(ApplyEditsRequest request, CancellationToken ct);
}
