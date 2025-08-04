using BBWM.Core.DTO;

namespace BBF.Reporting.Dashboard.DTO;

public class DashboardDTO : IDTO<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? UrlSlug { get; set; }
    public DateTime CreatedOn { get; set; }

    // Foreign keys and navigational properties.
    public string OwnerId { get; set; } = null!;
    public string? OwnerName { get; set; }
    public IList<int> OrganizationIds { get; set; } = new List<int>();
}