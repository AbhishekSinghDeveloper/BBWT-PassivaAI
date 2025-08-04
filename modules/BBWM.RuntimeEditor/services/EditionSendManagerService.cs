using BBWM.RuntimeEditor.interfaces;
using Microsoft.Extensions.Options;

namespace BBWM.RuntimeEditor.services;

public class EditionSendManagerService : IEditionSendManagerService
{
    readonly RuntimeEditorSettings runtimeEditorConfig;
    private readonly IEditionSenderGitApiService editionSenderGitApiService;
    private readonly IEditionSenderAwsApiService editionSenderAwsApiService;
    private readonly IEditionSenderLocalFolderService editionSenderLocalFolderService;

    public EditionSendManagerService(
        IOptionsSnapshot<RuntimeEditorSettings> runtimeEditorConfig,
        IEditionSenderGitApiService editionSenderGitApiService,
        IEditionSenderAwsApiService editionSenderAwsApiService,
        IEditionSenderLocalFolderService editionSenderLocalFolderService)
    {
        this.runtimeEditorConfig = runtimeEditorConfig.Value;
        this.editionSenderGitApiService = editionSenderGitApiService;
        this.editionSenderAwsApiService = editionSenderAwsApiService;
        this.editionSenderLocalFolderService = editionSenderLocalFolderService;
    }

    public async Task SendEditionUpdateToRepository(RteEditionUpdate editionUpdate, EditionSendProviderType sendProviderType, CancellationToken ct)
    {
        var editionUpdateRequest = new ApplyEditsRequest
        {
            ChangeCommitName = runtimeEditorConfig.EditsCommitName,
            EditJsonFilesPath = runtimeEditorConfig.EditJsonFilesPath,
            UserEmail = editionUpdate.SubmittedBy.Email,
            UserName = editionUpdate.SubmittedBy.Name,
            EditionUpdate = editionUpdate,
        };

        switch (sendProviderType)
        {
            case EditionSendProviderType.GitApi:
                await editionSenderGitApiService.SendEditionUpdate(editionUpdateRequest, ct);
                break;

            case EditionSendProviderType.AwsApi:
                await editionSenderAwsApiService.SendEditionUpdate(editionUpdateRequest, ct);
                break;

            case EditionSendProviderType.LocalFolder:
                await editionSenderLocalFolderService.SendEditionUpdate(editionUpdateRequest, ct);
                break;

            default:
                throw new InvalidOperationException("Edition sending provider type not defined");
        }
    }
}
