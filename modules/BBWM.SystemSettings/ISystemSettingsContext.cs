using BBWM.Core.Data;

using Microsoft.EntityFrameworkCore;

namespace BBWM.SystemSettings;

public interface ISystemSettingsContext : IDbContext
{
    DbSet<AppSettings> AppSettings { get; set; }
}
