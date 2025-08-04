using BBWM.Core.Data;
using Microsoft.EntityFrameworkCore.Design;

namespace BBWM.Demo.Data;

public class DesignTimeMySqlFactory : IDesignTimeDbContextFactory<MySqlDemoDataContext>
{
    public MySqlDemoDataContext CreateDbContext(string[] args) => DesignTimeDataContextFactory.CreateForMySql<MySqlDemoDataContext>("DemoMySqlConnection");
}
