using System.Text.Json.Serialization;

namespace BBWM.GitLab.Client;

public class FileData
{
    [JsonPropertyName("file_name")]
    public string Name { get; set; }

    [JsonPropertyName("file_path")]
    public string Path { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("encoding")]
    public string Encoding { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("ref")]
    public string Ref { get; set; }

    [JsonPropertyName("blob_id")]
    public string BlobId { get; set; }

    [JsonPropertyName("commit_id")]
    public string CommitId { get; set; }
}