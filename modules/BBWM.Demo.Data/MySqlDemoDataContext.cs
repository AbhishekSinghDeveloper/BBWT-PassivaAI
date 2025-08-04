using BBWM.Core.Services;
using BBWM.DbDoc.Core;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Demo.Data;

public class MySqlDemoDataContext : DemoDataContext, INamedContext
{
    public MySqlDemoDataContext(DbContextOptions<MySqlDemoDataContext> options, IDbServices dbServices) : base(options, dbServices)
    {
    }

    public string ContextName => "DemoContext";
}
