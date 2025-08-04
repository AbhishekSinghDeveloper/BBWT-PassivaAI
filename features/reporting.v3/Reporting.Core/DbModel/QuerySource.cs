using BBF.Reporting.Core.Enums;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;

namespace BBF.Reporting.Core.DbModel;

public class QuerySource : IAuditableEntity<Guid>
{
    public Guid Id { get; set; }

    public bool IsDraft { get; set; }

    public string? Name { get; set; }

    public DateTime CreatedOn { get; set; }

    public QueryFilterMode? FilterMode { get; set; }

    // Foreign keys and navigational properties.
    public string? OwnerId { get; set; }

    public User? Owner { get; set; }

    public ICollection<Organization> Organizations { get; set; } = new List<Organization>();

    /// <summary>
    /// A type of query (actually a string identifier) that determines a query source provider.
    /// By default, it's the Query Builder - the main building module of query's structure
    /// </summary>
    public string QueryType { get; set; } = null!;

    public QuerySource? ReleaseQuery { get; set; }

    public Guid? ReleaseQueryId { get; set; }
}