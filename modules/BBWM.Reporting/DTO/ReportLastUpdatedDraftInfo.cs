namespace BBWM.Reporting.DTO;

public class ReportLastUpdatedDraftInfo
{
    public Guid DraftId { get; set; }

    public string Owner { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }
}
