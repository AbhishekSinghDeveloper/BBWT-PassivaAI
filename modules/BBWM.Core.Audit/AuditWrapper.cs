using BBWM.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BBWM.Core.Audit;

public class AuditWrapper : IAuditWrapper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditContext _auditDataContext;

    private bool _auditEnabled = true;
    private AuditChangeEntry[] _auditEntries;

    public AuditWrapper(IAuditContext auditDataContext = null, IHttpContextAccessor httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _auditDataContext = auditDataContext;
    }

    public void OnBeforeSaveChanges(IEnumerable<EntityEntry> entries)
    {
        _auditEntries = GetAuditEntries(entries).ToArray();
    }

    public void DisableAudit() => this._auditEnabled = false;

    public Task OnAfterSaveChanges()
    {
        if (_auditEntries.Any())
        {
            foreach (var auditEntry in _auditEntries)
            {
                foreach (var prop in auditEntry.KeyProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.EntityId = prop.CurrentValue?.ToString();
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                var auditChanges = auditEntry.ToAudit();
                _auditDataContext.ChangeLogs.Add(auditChanges);
            }

            return _auditDataContext.SaveChangesAsync();
        }

        return Task.CompletedTask;
    }


    private IEnumerable<AuditChangeEntry> GetAuditEntries(IEnumerable<EntityEntry> entries)
    {
        if (!_auditEnabled) yield break;

        foreach (var entry in entries ?? Enumerable.Empty<EntityEntry>())
        {
            if ((entry.Entity is IAuditableEntity ||
                 entry.Entity.GetType().GetInterfaces().Any(interfaceItem =>
                    interfaceItem.IsGenericType && interfaceItem.GetGenericTypeDefinition() == typeof(IAuditableEntity<>))) &&
                entry.State != EntityState.Detached &&
                entry.State != EntityState.Unchanged)
            {
                yield return GetAuditEntry(entry);
            }
        }
    }

    private AuditChangeEntry GetAuditEntry(EntityEntry entry)
    {
        var auditEntry = new AuditChangeEntry(entry)
        {
            TableName = entry.Metadata.GetTableName(),
            EntityName = entry.Entity.GetType().Name
        };

        var user = _httpContextAccessor?.HttpContext?.User;
        if (user is not null)
        {
            auditEntry.UserName = user.Identity.Name;
        }

        SetAuditEntryByEntryProperties(entry, auditEntry);

        return auditEntry;
    }

    private static void SetAuditEntryByEntryProperties(EntityEntry entry, AuditChangeEntry auditEntry)
    {
        foreach (var property in entry.Properties)
        {
            var propertyName = property.Metadata.Name;

            if (property.Metadata.IsPrimaryKey())
            {
                auditEntry.EntityId = property.CurrentValue?.ToString();
            }
            else
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;

                    default:
                        // Other states are not handled
                        break;
                }
            }

            if (property.Metadata.IsKey() || property.Metadata.IsForeignKey())
            {
                auditEntry.KeyProperties.Add(property);
            }
        }
    }
}
