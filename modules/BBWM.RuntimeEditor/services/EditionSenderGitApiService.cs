using BBWM.GitLab;
using BBWM.GitLab.Client;
using BBWM.RuntimeEditor.interfaces;

using Microsoft.Extensions.Options;

using System.Text.Json;

namespace BBWM.RuntimeEditor.services;

public interface IEditionSenderGitApiService : IEditionSenderService
{ }

public class EditionSenderGitApiService : IEditionSenderGitApiService
{
    private readonly GitLabSettings gitLabSettings;
    private readonly RuntimeEditorSettings runtimeEditorConfig;
    private readonly IGitLabApiClient gitLabApiClient;

    public EditionSenderGitApiService(
        IOptionsSnapshot<RuntimeEditorSettings> runtimeEditorConfig,
        IOptionsSnapshot<GitLabSettings> gitLabSettings,
        IGitLabApiClient gitLabApiClient)
    {
        this.runtimeEditorConfig = runtimeEditorConfig.Value;
        this.gitLabSettings = gitLabSettings.Value;
        this.gitLabApiClient = gitLabApiClient;
    }

    public Task<bool> SendEditionUpdate(ApplyEditsRequest request, CancellationToken ct)
    {
        gitLabApiClient.Setup(gitLabSettings.GitLabApiUrl, gitLabSettings.GitLabApiToken, gitLabSettings.ProjectId);

        var action = new CommitAction
        {
            Action = "create",
            FilePath = $"{request.EditJsonFilesPath}{Guid.NewGuid()}.json",
            Content = JsonSerializer.Serialize(request.EditionUpdate, new JsonSerializerOptions { WriteIndented = true })
        };

        var result = gitLabApiClient.CreateCommit(new Commit
        {
            Branch = runtimeEditorConfig.EditsGitBranch,
            CommitMessage = request.ChangeCommitName,
            Actions = new List<CommitAction> { action },
            AuthorEmail = request.UserEmail,
            AuthorName = request.UserName
        });

        return Task.FromResult(result.IsSuccessStatusCode);
    }
}