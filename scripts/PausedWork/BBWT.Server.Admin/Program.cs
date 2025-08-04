
using AspNetCoreRateLimit;

using BBWM.SystemSettings;

using BBWT.Server.Admin;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Serilog;

using System.Diagnostics;
using System.Reflection;

var environment = Environment;
var productInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
var configuration = BuildConfiguration(args, environment, out var ebEnvironmentName);

ConfigureLogger(configuration);

if (!string.IsNullOrEmpty(ebEnvironmentName))
    environment = ebEnvironmentName;

Log.Logger.Information($"Starting application. {productInfo.ProductName} version {productInfo.ProductVersion}. Environment: {environment}");
if (IsDevelop)
{
    ProgramLogger.Debug("Config: {@config}", configuration.GetChildren());
    ProgramLogger.Information("Starting in development mode.");
}
else
    ProgramLogger.Debug("MainProduction()");

try
{

    var app = CreateWebAppBuilder(configuration, IsDevelop, environment, args).Build();

    using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
    {
        var sectionSettings = serviceScope.ServiceProvider.GetRequiredService<ISettingsSectionService>();
        var jsonOptions = serviceScope.ServiceProvider.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>();
        var startupLogger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Startup>>();

        Startup.Configure(app, sectionSettings, jsonOptions, startupLogger);

        // Seed policies
        var clientPolicyStore = serviceScope.ServiceProvider.GetRequiredService<IClientPolicyStore>();
        await clientPolicyStore.SeedAsync();

        var ipPolicyStore = serviceScope.ServiceProvider.GetRequiredService<IIpPolicyStore>();
        await ipPolicyStore.SeedAsync();
    }

    await app.RunAsync();

    ProgramLogger.Information("Application quit normally.");
}
catch (Exception e)
{
    ProgramLogger.Fatal(e, "Exception occured");
}
finally
{
    (ProgramLogger as IDisposable)?.Dispose();
}
