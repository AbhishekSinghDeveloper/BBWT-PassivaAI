using BBWM.Core.Services;

using Microsoft.EntityFrameworkCore.Design;

namespace BBWT.Data.SqlServer;

public class DesignTimeFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args) =>
        BBWM.Core.Data.DesignTimeDataContextFactory.CreateForSqlServer<DataContext>("DefaultConnection", new DbServices());
}
