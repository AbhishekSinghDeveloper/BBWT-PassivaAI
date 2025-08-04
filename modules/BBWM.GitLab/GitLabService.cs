using Microsoft.Extensions.Options;

using System.Text.Json;

namespace BBWM.GitLab;

public class GitLabService : IGitLabService
{
    private readonly GitLabSettings _gitLabConfig;
    private readonly IHttpClientFactory _httpClientFactory;

    public GitLabService(
        IOptionsSnapshot<GitLabSettings> gitLabConfig,
        IHttpClientFactory httpClientFactory)
    {
        _gitLabConfig = gitLabConfig.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> Push(string function, string content, string username, CancellationToken cancellationToken = default)
    {
        var dataObj = new AwsPostDTO
        {
            projectId = _gitLabConfig.ProjectId,
            sourceBranch = _gitLabConfig.Branch,
            username = username,
            content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content)),
            function = function
        };

        return await DoRequest("gitpush", dataObj, cancellationToken);
    }

    public async Task<bool> Run(string action, string content, string username, CancellationToken cancellationToken = default)
    {
        var dataObj = new AwsPostDTO
        {
            projectId = _gitLabConfig.ProjectId,
            sourceBranch = _gitLabConfig.Branch,
            username = username,
            content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content))
        };

        return await DoRequest(action, dataObj, cancellationToken);
    }

    private async Task<bool> DoRequest(string path, object dataObject, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient();
        var serializedData = JsonSerializer.Serialize(dataObject);
        client.DefaultRequestHeaders.Add("X-Api-Key", _gitLabConfig.AwsApiToken);
        var stringContent = new StringContent(serializedData, System.Text.Encoding.UTF8, "application/json");
        var url = _gitLabConfig.AwsApiUrl.TrimEnd('/') + "/" + path;
        var response = await client.PostAsync(url, stringContent, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}