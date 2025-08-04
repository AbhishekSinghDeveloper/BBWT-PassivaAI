using BBWM.Core.Data;

namespace BBWM.Core.Audit;

/// <summary>
/// Change log item definition
/// </summary>
public class ChangeLogItem : IEntity
{
    public int Id { get; set; }

    public string PropertyName { get; set; }

    public string OldValue { get; set; }

    public string NewValue { get; set; }

    public int ChangeLogId { get; set; }

    public virtual ChangeLog ChangeLog { get; set; }
}
