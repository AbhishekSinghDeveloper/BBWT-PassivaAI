using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using BBWM.AWS;
using BBWM.Core.Loggers;
using BBWM.Core.Web.Extensions;

using BBWT.Server.Admin;

using Destructurama;

using Serilog;

using ISerilogLogger = Serilog.ILogger;

partial class Program
{
    private static string Environment =>
           System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development;

    private static bool IsDevelop =>
        string.Equals(Environment, Environments.Development, StringComparison.OrdinalIgnoreCase);

    private static Startup Startup { get; set; }

    private static ISerilogLogger ProgramLogger { get; set; }

    private static WebApplicationBuilder CreateWebAppBuilder(IConfiguration configuration, bool detailedErrors, string environment, string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        Startup = new Startup(configuration, builder.Environment);

        Startup.ConfigureServices(builder.Services);

        builder.Host
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(Startup.ConfigureContainer)
            .UseSerilog(dispose: true);

        builder.WebHost
            .CaptureStartupErrors(true)
            .UseSetting(WebHostDefaults.DetailedErrorsKey, detailedErrors.ToString())
            .UseConfiguration(configuration)
            .UseEnvironment(environment);

        builder.Host
            .ConfigureLogging(logging => logging.AddSerilog())
            .ConfigureServices(s => s.AddSingleton(configuration));

        SetupStorageProvider(builder, configuration);

        return builder;
    }

    private static void SetupStorageProvider(WebApplicationBuilder builder, IConfiguration configuration)
    {
        var configurationProviderName =
            configuration.GetSection("StorageSettings")?.GetValue<string>("ProviderName");

        switch (configurationProviderName)
        {
            case "AWS":
                var awsSettings = configuration.GetSection("AwsSettings").Get<AwsSettings>();
                if (awsSettings is not null &&
                    !string.IsNullOrEmpty(awsSettings.AccessKeyId) &&
                    !string.IsNullOrEmpty(awsSettings.SecretAccessKey) &&
                    !string.IsNullOrEmpty(awsSettings.AwsRegion) &&
                    !string.IsNullOrEmpty(awsSettings.ParametersPath))
                {
                    builder.Host.ConfigureAppConfiguration(configBuilder =>
                    {
                        configBuilder.AddSystemsManager(configureSource =>
                        {
                            configureSource.AwsOptions = new AWSOptions
                            {
                                Credentials = new BasicAWSCredentials(awsSettings.AccessKeyId, awsSettings.SecretAccessKey),
                                Region = RegionEndpoint.GetBySystemName(awsSettings.AwsRegion)
                            };
                            configureSource.Path = awsSettings.ParametersPath;
                            if (awsSettings.ParametersReloadingInterval is not null)
                                configureSource.ReloadAfter = TimeSpan.FromSeconds((double)awsSettings.ParametersReloadingInterval);
                        });
                    });
                }
                break;
            case "Azure":
            default:
                throw new NotImplementedException();
        }
    }

    private static IConfiguration BuildConfiguration(string[] args, string environment, out string ebEnvironmentName) =>
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{environment}.json", true, true)
            .AddEnvironmentVariables()
            .AddEbConfig(out ebEnvironmentName)
            .Build();

    private static void ConfigureLogger(IConfiguration configuration)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Destructure.UsingAttributes();

        if (!IsDevelop)
        {
            loggerConfig.Enrich.WithDockerContainerId();
            loggerConfig.ConfigureLogentries(configuration);
            loggerConfig.ConfigureGraylog(configuration);
        }

        Log.Logger = loggerConfig.CreateLogger();
        ProgramLogger = Log.Logger.ForContext<Program>();
    }
}
