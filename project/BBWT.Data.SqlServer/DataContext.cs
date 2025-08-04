using BBWM.Core.Services;

using Microsoft.EntityFrameworkCore;

namespace BBWT.Data.SqlServer;

/// <summary>
/// Main data context of the project.
/// </summary>
/// <remarks>
/// <para>
/// Run these commands in ./ folder (root of the project).
/// </para>
/// Add migration:
/// <code>
///     dotnet ef migrations add MIGRATION_NAME -p project/BBWT.Data.SqlServer -s project/bbwt.server -c BBWT.Data.SqlServer.DataContext
/// </code>
/// Run migrations:
/// <code>
///     dotnet ef database update -p project/BBWT.Data.SqlServer -s project/bbwt.server -c BBWT.Data.SqlServer.DataContext
/// </code>
/// </remarks>
public class DataContext : DataContextBase
{
    public DataContext(DbContextOptions<DataContext> options, IDbServices dbServices) : base(options, dbServices)
    {
    }
}
