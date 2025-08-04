using BBWM.Core.Data;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Audit;

public interface IAuditContext : IDbContext
{
    DbSet<ChangeLog> ChangeLogs { get; set; }

    DbSet<ChangeLogItem> ChangeLogItems { get; set; }
}
