using BBWM.Core.Services;

using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace BBWT.Data.MySQL;

/// <summary>
/// Main data context of the project
/// </summary>
/// <remarks>
/// <para>
/// Run these commands in ./ folder (root of the project).
/// </para>
/// Add migration:
/// <code>
///     dotnet ef migrations add MIGRATION_NAME -p project/bbwt.data.mysql -s project/bbwt.server -c BBWT.Data.MySQL.DataContext
/// </code>
/// Run migrations:
/// <code>
///     dotnet ef database update -p project/bbwt.data.mysql -s project/bbwt.server -c BBWT.Data.MySQL.DataContext
/// </code>
/// IMPORTANT! A note about choosing a database's (and its entities) CHARSET.
/// This question is MySQL-specific, because since we've upgraded to
/// .NET5, we got that migration classes started to define charsets in
/// their code explicitly. That's because of changes in Pomelo.EntityFramework MySQL provider's package
/// forced by .NET5 upgrade.
/// It's maybe that your decide to keep using the same charset as in out migrations code.
/// If not, then it's solved by running these steps:
///<list type="number">
///<item>
///Re-define a charset inside OnModelCreating(...), e.g.:
///     <i><code>builder.HasCharSet("latin1" /*a sample charset*/);</code></i>
///</item>
///<item>
///Drop all existing migrations and recreate them from scratch with a new one.
///The very first migration we name "Initial".
///</item>
///</list>
/// </remarks>
public class DataContext : DataContextBase
{
    public DataContext(DbContextOptions<DataContext> options, IDbServices dbServices) : base(options, dbServices)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasCharSet(CharSet.Utf8Mb4, false);
        base.OnModelCreating(builder);
    }
}
