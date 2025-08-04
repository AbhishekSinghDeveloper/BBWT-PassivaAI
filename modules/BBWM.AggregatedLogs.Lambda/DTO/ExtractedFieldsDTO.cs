using System.Text.Json.Serialization;

namespace BBWM.AggregatedLogs.Lambda.DTO;

internal class ExtractedFieldsDTO
{
    public string Event { get; set; }

    [JsonPropertyName("request_id")]
    public string RequestId { get; set; }

    public string Type { get; set; }

    public string Timestamp { get; set; }
}
