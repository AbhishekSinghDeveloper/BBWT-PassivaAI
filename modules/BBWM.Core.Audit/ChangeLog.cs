using BBWM.Core.Data;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Audit;

/// <summary>
/// Change log definition
/// </summary>
public class ChangeLog : IEntity
{
    public int Id { get; set; }

    public EntityState State { get; set; }

    public DateTime DateTime { get; set; }

    public string EntityName { get; set; }

    public string TableName { get; set; }

    public string EntityId { get; set; }

    public string UserName { get; set; }

    public virtual IList<ChangeLogItem> ChangeLogItems { get; set; }
}
