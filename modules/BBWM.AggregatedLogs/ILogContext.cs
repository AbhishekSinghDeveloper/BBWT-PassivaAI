using BBWM.Core.Data;

using Microsoft.EntityFrameworkCore;

namespace BBWM.AggregatedLogs;

public interface ILogContext : IDbContext
{
    DbSet<Log> Logs { get; set; }
}
