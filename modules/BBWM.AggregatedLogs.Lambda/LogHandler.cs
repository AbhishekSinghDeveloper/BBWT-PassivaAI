using Amazon.Lambda.Core;
using BBWM.AggregatedLogs.Lambda.DTO;
using BBWM.Core.Data;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BBWM.AggregatedLogs.Lambda;

public class LogHandler
{
    private readonly ServiceProvider _serviceProvider;

    public LogHandler()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        CreateDatabase();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ILambdaLogService, LambdaLogService>();
        services.AddSingleton<ILogParser, AmazonLambdaLogParser>();

        var environment = Environment.GetEnvironmentVariable("Environment");

        var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

        AddDataContext(services, configuration);
    }

    private void AddDataContext(IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(LogLambdaDatabaseSettings.SettingsSectionName).Get<LogLambdaDatabaseSettings>()
                ?? new LogLambdaDatabaseSettings();

        services.AddDbContext<ILogContext, LogContext>(
            settings.GetDbContextOptionsBuilder<LogContext>(settings.ConnectionString).Options);
    }

    private void CreateDatabase()
    {
        using var scope = _serviceProvider.CreateScope();
        var logContext = scope.ServiceProvider.GetService<ILogContext>();
        logContext.Database.EnsureCreated();
    }

    public async Task ProcessLogs(EventDTO input, ILambdaContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetService<ILambdaLogService>();
        await service.ProcessLogs(input);
    }
}
