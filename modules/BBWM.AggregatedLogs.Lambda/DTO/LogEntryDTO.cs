namespace BBWM.AggregatedLogs.Lambda.DTO;

internal class LogEntryDTO
{
    public string Id { get; set; }

    public long Timestamp { get; set; }

    public string Message { get; set; }

    public ExtractedFieldsDTO ExtractedFields { get; set; }
}
