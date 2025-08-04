using BBWM.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace BBWM.AggregatedLogs;

public class LogContext : DbContextCore, ILogContext
{
    public LogContext(DbContextOptions<LogContext> options) : base(options)
    {
    }

    public DbSet<Log> Logs { get; set; }
}
