using BBWM.Core.Data;
using Microsoft.EntityFrameworkCore.Design;

namespace BBWM.Demo.Data;

public class DesignTimeSqlServerFactory : IDesignTimeDbContextFactory<SqlServerDemoDataContext>
{
    public SqlServerDemoDataContext CreateDbContext(string[] args) => DesignTimeDataContextFactory.CreateForSqlServer<SqlServerDemoDataContext>("DemoConnection");
}
