using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace BBWM.Core.Data
{
    public static class DesignTimeDataContextFactory
    {
        public static TContext CreateForMySql<TContext>(string connectionStringName, params object[] extraArguments) where TContext : DbContext
        {
            var connectionString = GetConnectionString(connectionStringName);

            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            var args = new List<object> { builder.Options };

            if (extraArguments is not null)
            {
                args.AddRange(extraArguments);
            }

            return (TContext)Activator.CreateInstance(
                typeof(TContext),
                BindingFlags.CreateInstance | BindingFlags.OptionalParamBinding,
                null,
                args.ToArray(),
                CultureInfo.CurrentCulture);
        }

        public static TContext CreateForSqlServer<TContext>(string connectionStringName, params object[] extraArguments) where TContext : DbContext
        {
            var connectionString = GetConnectionString(connectionStringName);

            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseSqlServer(connectionString);

            var args = new List<object> { builder.Options };

            if (extraArguments is not null)
            {
                args.AddRange(extraArguments);
            }

            return (TContext)Activator.CreateInstance(
                typeof(TContext),
                BindingFlags.CreateInstance | BindingFlags.OptionalParamBinding,
                null,
                args.ToArray(),
                CultureInfo.CurrentCulture);
        }


        private static string GetConnectionString(string connectionStringName)
        {
            var basePath = Directory.GetCurrentDirectory();

            Trace.WriteLine($"BasePath: {basePath}");

            var configuration = new ConfigurationBuilder()
               .SetBasePath(basePath)
               .AddJsonFile("appsettings.json")
               .AddJsonFile($"appsettings.{AppEnvironment.AppEnvironment.Environment}.json")
               .Build();

            return configuration.GetConnectionString(connectionStringName);
        }
    }
}
