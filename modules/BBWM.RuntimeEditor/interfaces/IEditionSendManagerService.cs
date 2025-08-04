namespace BBWM.RuntimeEditor.interfaces;

public interface IEditionSendManagerService
{
    Task SendEditionUpdateToRepository(RteEditionUpdate editionUpdate, EditionSendProviderType sendProviderType, CancellationToken ct);
}
