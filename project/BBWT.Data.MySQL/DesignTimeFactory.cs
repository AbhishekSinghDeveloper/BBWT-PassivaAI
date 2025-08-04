using BBWM.Core.Services;

using Microsoft.EntityFrameworkCore.Design;

namespace BBWT.Data.MySQL;

public class DesignTimeFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args) =>
        BBWM.Core.Data.DesignTimeDataContextFactory.CreateForMySql<DataContext>("MySqlConnection", new DbServices());
}
