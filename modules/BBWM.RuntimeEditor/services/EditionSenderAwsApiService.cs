using BBWM.GitLab;
using BBWM.RuntimeEditor.interfaces;

using System.Text.Json;

namespace BBWM.RuntimeEditor.services;

public interface IEditionSenderAwsApiService : IEditionSenderService
{ }

public class EditionSenderAwsApiService : IEditionSenderAwsApiService
{
    private readonly IGitLabService gitLabService;

    public EditionSenderAwsApiService(IGitLabService gitLabService)
        => this.gitLabService = gitLabService;

    public async Task<bool> SendEditionUpdate(ApplyEditsRequest request, CancellationToken ct)
        => await gitLabService.Run("applyedits", JsonSerializer.Serialize(request), request.UserEmail, ct);
}