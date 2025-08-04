using BBWM.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BBWM.Core.Audit;

/// <summary>
/// Audit entry definition
/// </summary>
public class AuditChangeEntry
{
    private const int SavedPropertyLengthLimit = 2000;

    public AuditChangeEntry(EntityEntry entry)
    {
        State = entry.State;
    }

    public EntityState State { get; set; }
    public string TableName { get; set; }
    public string EntityName { get; set; }
    public string UserName { get; set; }
    public string EntityId { get; set; }
    public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
    public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
    public List<PropertyEntry> KeyProperties { get; } = new List<PropertyEntry>();

    public ChangeLog ToAudit() =>
        new()
        {
            State = State,
            UserName = UserName,
            TableName = TableName,
            EntityName = EntityName,
            DateTime = DateTime.UtcNow,
            EntityId = EntityId,
            ChangeLogItems = NewValues.Keys
            .Select(k => new ChangeLogItem
            {
                NewValue = ValueToLimitedString(NewValues[k]),
                OldValue = OldValues.ContainsKey(k) ? ValueToLimitedString(OldValues[k]) : null,
                PropertyName = k
            })
            .Where(x => State != EntityState.Modified || x.NewValue != x.OldValue)
            .ToArray()
        };

    private static string ValueToLimitedString(object p)
    {
        if (p is null) return null;
        var s = p.ToString();
        return s.Length <= SavedPropertyLengthLimit ? s : s.MaxLength(SavedPropertyLengthLimit) + " ...";
    }
}
