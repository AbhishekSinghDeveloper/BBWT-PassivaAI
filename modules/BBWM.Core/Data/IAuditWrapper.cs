using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BBWM.Core.Data;

public interface IAuditWrapper
{
    void OnBeforeSaveChanges(IEnumerable<EntityEntry> entries);

    void DisableAudit();

    Task OnAfterSaveChanges();
}
