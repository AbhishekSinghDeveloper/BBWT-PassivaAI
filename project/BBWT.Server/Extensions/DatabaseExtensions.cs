using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.ModuleLinker;
using BBWM.SystemSettings;
using BBWT.Data;
using BBWT.Data.MySQL;
using BBWT.Data.SqlServer;
using BBWT.InitialData;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OpenSearch.Client;

namespace BBWT.Server.Extensions;

public static class DatabaseExtensions
{
    public static void AddProjectDataContexts(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseConnectionSettings = configuration.GetDatabaseConnectionSettings();

        if (databaseConnectionSettings.DatabaseType == DatabaseType.MySql)
        {
            var connectionString = configuration.GetConnectionString("MySqlConnection");

            services.AddBBWTMySQLDataContext(databaseConnectionSettings, connectionString, IdentityOptions,
                ConfigureContextWarningBuilder);
        }
        else if (databaseConnectionSettings.DatabaseType == DatabaseType.MsSql)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddBBWTSqlServerDataContext(databaseConnectionSettings, connectionString, IdentityOptions,
                ConfigureContextWarningBuilder);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public static void AddModulesDataContexts(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionSettings = configuration.GetDatabaseConnectionSettings();

        //Module linker: data contexts
        ModuleLinker.RunLinkers<IDataContextModuleLinkage>(
            linker => linker.AddDataContext(services, configuration, connectionSettings));
    }

    public static void InitDatabases(this IApplicationBuilder app,
        IConfiguration configuration,
        IHostApplicationLifetime applicationLifetime,
        IWebHostEnvironment environment,
        bool isMigrationsAppRun,
        ILogger<Startup> logger)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

        #region Migrations
        // Instruction to apply new migrations on application start. In most project cases we suppose it to be
        // triggered as a part of CI/CD process. Instead, in rare cases some projects can do it only on application start.
        // (This option is defined in app settings)
        var runMigrationsOnStart = configuration.GetValue<bool>("RunMigrationsOnStart");

        // Migrations & Seeding
        if (runMigrationsOnStart || isMigrationsAppRun)
        {
            // Main database
            var dataContext = serviceScope.ServiceProvider.GetService<IDataContext>();
            var mainDatabase = dataContext.Database;
            try
            {
                mainDatabase.Migrate();
            }
            catch (Exception ex)
            {
                var exMessage = string.Join(
                    "\n",
                    "Main database migration failure.",
                    $"DATABASE NAME: {mainDatabase.GetDbConnection().Database}",
                    $"DATABASE TYPE: {configuration.GetDatabaseConnectionSettings().DatabaseType}\n",
                    "APPLIED MIGRATIONS:",
                    $"[ {string.Join(",  ", mainDatabase.GetAppliedMigrations().ToArray())} ]\n",
                    "PENDING MIGRATIONS:",
                    $"[ {string.Join(",  ", mainDatabase.GetPendingMigrations().ToArray())} ]");

                throw new DatabaseMigrationException(exMessage, ex) { IsMigrationsAppRun = isMigrationsAppRun };
            }


            #region Initialization of custom databases of the project
            // Block for initialization of custom databases of the project, triggered by the same rules as the main database
            // ...
            #endregion
        }


        #region Additional Databases

        // Unlike the main database which initialization (triggered via the migration job's script OR on the startup of the
        // running application with RunMigrationsOnStart=true OR --migrate argument passed), these additional databases of this block
        // are always created on startup of the application.
        // Reasons:
        //   - database like Audit created with EnsureCreated, not with migrations (probably - to be reworked to migrations then)
        //   - removable databases like Demo are not supposed to be hardcoded in the migration job of YML and therefore placed in BBWM
        //     module and triggered on the startup

        //Module linker: DB creation
        ModuleLinker.RunLinkers<IDbCreateModuleLinkage>(linker => linker.Create(serviceScope));
        #endregion
        #endregion
    }

    public static void EnsureApplicationInitialData(this IApplicationBuilder app,
        IConfiguration configuration,
        IHostApplicationLifetime applicationLifetime,
        IWebHostEnvironment environment,
        ILogger<Startup> logger)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

        // getting a flag to find out if it's a first ever start to initialize once seeded data
        var settingsService = serviceScope.ServiceProvider.GetService<ISettingsService>();
        var appInitializationSection = settingsService.GetSettingsSection<AppInitializationSettings>();
        bool includingOnceSeededData = !(appInitializationSection.OnceSeededDataInitialized ?? false);

        //Module linker: Ensures initial data
        ModuleLinker.RunLinkers<IInitialDataModuleLinkage>(
            linker => linker.EnsureInitialData(serviceScope, includingOnceSeededData).Wait());

        // Ensures customer project initial data
        try
        {
            serviceScope.EnsureInitialData(includingOnceSeededData);
        }
        catch (DataInitCriticalException ex)
        {
            ModuleLinker.AddCommonException(ex);
        }

        // Set a flag that once seeded data is initialized forever
        if (includingOnceSeededData)
        {
            appInitializationSection.OnceSeededDataInitialized = true;
            settingsService.SaveSettingsSection(appInitializationSection);
        }
    }

    // Configure Identity options
    private static void IdentityOptions(IdentityOptions options)
    {
        options.SignIn.RequireConfirmedEmail = false;

        // Lockout settings by default
        options.Lockout.AllowedForNewUsers = false;
        options.Lockout.MaxFailedAccessAttempts = 100;

        // Password settings
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 0;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    }

    /// <summary>
    /// Configures main DB context warnings for both MySQL and MsSQL types
    /// </summary>
    /// <param name="builder"></param>
    private static void ConfigureContextWarningBuilder(WarningsConfigurationBuilder builder)
    {
        // This warning forced to be ignored, otherwise we get multiple warning for LINQ DB queries which contain
        // multiple includes. EF suggests to use .AsSplitQuery() for a LINQ query, but managing query's includes
        // is a project logic's specific task.
        // By default we ignore it as disturbing, but feel free to comment it out and see warnings for your complext
        // LINQ queries.
        builder.Ignore(RelationalEventId.MultipleCollectionIncludeWarning);
    }
}
