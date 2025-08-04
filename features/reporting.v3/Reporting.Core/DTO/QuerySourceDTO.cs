using BBF.Reporting.Core.Enums;
using BBWM.Core.DTO;

namespace BBF.Reporting.Core.DTO;

public class QuerySourceDTO : IDTO<Guid>
{
    public Guid Id { get; set; }

    public bool IsDraft { get; set; }

    public string? Name { get; set; }

    public DateTime CreatedOn { get; set; }

    public QueryFilterMode? FilterMode { get; set; }

    // Foreign keys and navigational properties.
    public string? OwnerId { get; set; }

    public string? OwnerName { get; set; }

    public string QueryType { get; set; } = null!;

    public Guid? ReleaseQueryId { get; set; }

    public IList<int> OrganizationIds { get; set; } = new List<int>();
}