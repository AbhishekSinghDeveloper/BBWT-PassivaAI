using AspNetCoreRateLimit;
using BBWM.Core.AppEnvironment;
using BBWM.SystemSettings;

using BBWT.Server;
using BBWT.Server.Extensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Serilog;

using System.Diagnostics;
using System.Reflection;

var environment = AppEnvironment.Environment;
var productInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
var configuration = BuildConfiguration(args, environment, out var ebEnvironmentName);

ConfigureLogger(configuration);

if (!string.IsNullOrEmpty(ebEnvironmentName))
{
    environment = ebEnvironmentName;
}

// Log startup info
Log.Logger.Information(
    @$"Starting application:
       product: {productInfo.ProductName},
       version: {productInfo.ProductVersion},
       hosting environment: {environment}");

if (AppEnvironment.IsDevelopment)
{
    ProgramLogger.Debug("Configuration: {@config}", configuration.GetChildren());
}

try
{
    var app = CreateWebAppBuilder(configuration, AppEnvironment.IsDevelopment, environment, args).Build();

    using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
    {
        var sectionSettings = serviceScope.ServiceProvider.GetRequiredService<ISettingsSectionService>();
        var jsonOptions = serviceScope.ServiceProvider.GetRequiredService<IOptions<JsonOptions>>();
        var startupLogger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Startup>>();

        Startup.Configure(app, app.Lifetime, sectionSettings, jsonOptions, startupLogger);

        // Seed policies
        var clientPolicyStore = serviceScope.ServiceProvider.GetRequiredService<IClientPolicyStore>();
        await clientPolicyStore.SeedAsync();

        var ipPolicyStore = serviceScope.ServiceProvider.GetRequiredService<IIpPolicyStore>();
        await ipPolicyStore.SeedAsync();
    }

    Playground.OnAppRun(app);

    await app.RunAsync();

    ProgramLogger.Information("Application quit normally.");
}
catch (Exception ex)
{
    Playground.OnAppException(ex);

    // The EF CLI throws a starts up the host to read database configuration and then throws a StopTheHost
    // exception to stop the host from doing anything else. Checking for that expected exception so that
    // it is not logged as a fatal error.
    if (ex.GetType().Name == "StopTheHostException")
        throw;

    ProgramLogger.Fatal(ex, "Exception occured.");

    // In case app startup fails due to a migration error that was triggered by the CI migration job
    // we set a non-zero exit code so the CI job fails and developers can get a proper notification
    if (ex is DatabaseMigrationException me && me.IsMigrationsAppRun)
        System.Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
    (ProgramLogger as IDisposable)?.Dispose();
}