using BBWM.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Audit;

public class AuditContext : DbContextCore, IAuditContext
{
    public AuditContext(DbContextOptions<AuditContext> options) : base(options)
    {
    }

    public DbSet<ChangeLog> ChangeLogs { get; set; }
    public DbSet<ChangeLogItem> ChangeLogItems { get; set; }
}