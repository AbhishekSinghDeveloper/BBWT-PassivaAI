using BBWM.Core.Data;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Menu.Db;

public interface IMenuContext : IDbContext
{
    DbSet<MenuItem> Menu { get; set; }
    DbSet<FooterMenuItem> FooterMenuItems { get; set; }
}
