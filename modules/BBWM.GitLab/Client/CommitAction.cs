using System.Text.Json.Serialization;

namespace BBWM.GitLab.Client;

public class CommitAction
{
    /// <summary>
    /// The action to perform, create, delete, move, update
    /// </summary>
    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("file_path")]
    public string FilePath { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}