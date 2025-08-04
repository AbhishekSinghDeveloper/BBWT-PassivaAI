using System.Text.Json.Serialization;

namespace BBWM.GitLab.Client;

public class Commit
{
    [JsonPropertyName("branch")]
    public string Branch { get; set; }

    [JsonPropertyName("commit_message")]
    public string CommitMessage { get; set; }

    [JsonPropertyName("commit_description")]
    public string CommitDescription { get; set; }

    [JsonPropertyName("actions")]
    public List<CommitAction> Actions { get; set; }

    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; }

    [JsonPropertyName("author_email")]
    public string AuthorEmail { get; set; }
}