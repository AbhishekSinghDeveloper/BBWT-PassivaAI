using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BBWM.Core.Data;

namespace BBWM.SSRS
{
    public class DbContextModuleLinkage : IDbContextModuleLinkage
    {
        public IServiceCollection AddDbContext(DatabaseConnectionSettings connectionSettings, IConfiguration configuration, IServiceCollection services)
        {
            var connectionString = GetConnectionString(configuration, "SsrsConnection");
            services.AddDbContext<SsrsDataContext>(options =>
                options.UseSqlServer(connectionString, builder => builder.EnableRetryOnFailure(connectionSettings)));
            services.AddScoped(typeof(ISsrsDataContext), typeof(SsrsDataContext));

            return services;
        }

        private string GetConnectionString(IConfiguration configuration, string settingName)
        {
            var connectionString = configuration.GetConnectionString(settingName);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new System.Exception($"SSRS module: no connection string in {settingName} setting for data base");
            }
            return connectionString;
        }
    }
}