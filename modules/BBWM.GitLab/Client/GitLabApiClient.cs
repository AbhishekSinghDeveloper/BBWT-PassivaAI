using BBWM.Core.Utils;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace BBWM.GitLab.Client;

public class GitLabApiClient : IGitLabApiClient
{
    private string _baseUrl;
    private string _privateToken;

    public IGitLabApiClient Setup(string gitLabApiUrl, string privateToken, string projectId)
    {
        _privateToken = privateToken;
        _baseUrl = gitLabApiUrl + "/projects/" + UrlEncoder.Default.Encode(projectId);
        return this;
    }

    public bool BranchExists(string branchName)
    {
        var resp = SendGetRequest("/repository/branches/" + UrlEncoder.Default.Encode(branchName), new Dictionary<string, string>());

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            return true;
        }
        else if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        else
        {
            throw new Exception(resp.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }
    }

    public void CreateBranch(string branchName, string branchNameFrom)
    {
        var qParams =
            new Dictionary<string, string>
            {
                    { "branch", branchName},
                    { "ref", branchNameFrom}
            };

        SendPostRequest("/repository/branches", qParams);
    }

    public HttpResponseMessage CreateCommit(Commit commit)
        => SendPostRequest("/repository/commits", new Dictionary<string, string>(), commit);

    public bool MergeRequestExists(string branchName, string targetBranch)
    {
        var qParams =
            new Dictionary<string, string>
            {
                    { "state", "opened"},
                    { "source_branch", branchName},
                    { "target_branch", targetBranch}
            };

        var resp = SendGetRequest("/merge_requests", qParams);

        if (resp.IsSuccessStatusCode)
        {
            var data = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var array = JsonSerializer.Deserialize<List<object>>(data);

            return array.Count > 0;
        }

        throw new Exception(resp.Content.ReadAsStringAsync().GetAwaiter().GetResult());
    }

    public void CreateMergeRequest(MergeRequest mergeRequest)
    {
        SendPostRequest("/merge_requests", new Dictionary<string, string>(), mergeRequest);
    }

    public bool FileExists(string branchName, string filePath)
    {
        var qParams =
            new Dictionary<string, string>
            {
                    { "ref", branchName}
            };

        var resp = SendGetRequest("/repository/files/" + UrlEncoder.Default.Encode(filePath), qParams);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            return true;
        }
        else if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        else
        {
            throw new Exception(resp.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }
    }

    public FileData GetFile(string branchName, string filePath)
    {
        var qParams =
            new Dictionary<string, string>
            {
                    { "ref", branchName}
            };

        var resp = SendGetRequest("/repository/files/" + UrlEncoder.Default.Encode(filePath), qParams);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonSerializer.Deserialize<FileData>(json, JsonSerializerOptionsProvider.Options);
        }
        else if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        else
        {
            throw new Exception(resp.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }
    }

    private HttpResponseMessage SendGetRequest(string url, Dictionary<string, string> queryParams)
    {
        using var client = new HttpClient();
        var qs = QueryString.Create(queryParams);
        var uri = _baseUrl + url + qs.ToUriComponent();

        client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", _privateToken);

        return client.GetAsync(uri).GetAwaiter().GetResult();
    }

    private HttpResponseMessage SendPostRequest(string url, Dictionary<string, string> queryParams, object content = null)
    {
        using var client = new HttpClient();
        var qs = QueryString.Create(queryParams);
        var uri = _baseUrl + url + qs.ToUriComponent();

        client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", _privateToken);

        HttpContent httpContent = null;

        if (content is not null)
        {
            httpContent = new StringContent(JsonSerializer.Serialize(content));
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        var resp = client.PostAsync(uri, httpContent).GetAwaiter().GetResult();

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception(resp.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }

        return resp;
    }
}