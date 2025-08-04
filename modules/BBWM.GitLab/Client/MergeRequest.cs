using System.Text.Json.Serialization;

namespace BBWM.GitLab.Client;

public class MergeRequest
{
    [JsonPropertyName("source_branch")]
    public string SourceBranch { get; set; }

    [JsonPropertyName("target_branch")]
    public string TargetBranch { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// Flag indicating if a merge request should remove the source branch when merging
    /// </summary>
    [JsonPropertyName("remove_source_branch")]
    public bool RemoveSourceBranch { get; set; }
}