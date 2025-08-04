using BBWM.Core.Services;
using BBWM.DbDoc.Core;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Demo.Data;

public class SqlServerDemoDataContext : DemoDataContext, INamedContext
{
    public SqlServerDemoDataContext(DbContextOptions<SqlServerDemoDataContext> options, IDbServices dbServices) : base(options, dbServices)
    {
    }

    public string ContextName => "DemoContext";
}
