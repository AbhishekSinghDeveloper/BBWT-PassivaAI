using BBWM.Core.DTO;

namespace BBF.Reporting.Core.DTO;

public class WidgetSourceDTO : IDTO<Guid>
{
    public Guid Id { get; set; }

    public bool IsDraft { get; set; }

    public string WidgetType { get; set; } = null!;

    public string? Name { get; set; }

    public string? Title { get; set; }

    public string? Code { get; set; }

    public DateTime CreatedOn { get; set; }

    // Foreign keys and navigational properties.
    public Guid? ReleaseWidgetId { get; set; }

    public string? OwnerId { get; set; }

    public string? OwnerName { get; set; }

    public IList<int> OrganizationIds { get; set; } = new List<int>();

    public int? DisplayRuleId { get; set; }

    public VariableRuleDTO? DisplayRule { get; set; }
}