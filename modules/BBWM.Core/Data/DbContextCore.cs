using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Data
{
    public class DbContextCore : DbContext, IDbContext
    {
        public DbContextCore(DbContextOptions options) : base(options)
        {
        }
    }
}
