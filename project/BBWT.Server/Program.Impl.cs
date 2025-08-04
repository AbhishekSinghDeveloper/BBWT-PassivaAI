using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using BBWM.AWS;
using BBWM.Core.AppEnvironment;
using BBWM.Core.Loggers;
using BBWM.Core.Web.Extensions;

using BBWT.Server;
using Destructurama;

using Serilog;

using System.Security.Authentication;

using ISerilogLogger = Serilog.ILogger;

partial class Program
{
    private static Startup Startup { get; set; }

    private static ISerilogLogger ProgramLogger { get; set; }

    private static WebApplicationBuilder CreateWebAppBuilder(IConfiguration configuration, bool detailedErrors, string environment, string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            EnvironmentName = environment,
        });
        Startup = new Startup(configuration, builder.Environment);

        Startup.ConfigureServices(builder.Services);

        builder.Host
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(Startup.ConfigureContainer)
            .UseSerilog(dispose: true);

        builder.WebHost
            .UseConfiguration(configuration)
            .UseSetting(WebHostDefaults.DetailedErrorsKey, detailedErrors.ToString())
            .ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 104857600;
                options.AddServerHeader = false;
                options.ConfigureHttpsDefaults(
                    httpsOptions => httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13);
            })
            .CaptureStartupErrors(true);

        builder.Host
            .ConfigureLogging(logging => logging.AddSerilog());

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
                            configureSource.Optional = true;
                            if ((awsSettings.ParametersReloadingInterval ?? 0) > 0)
                                configureSource.ReloadAfter = TimeSpan.FromSeconds((double)awsSettings.ParametersReloadingInterval);
                        });
                    });
                }
                break;
            case "Azure":
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

    /// <summary>
    /// Sets up a logging system based on the application configuration.
    /// Serilog library provides diagnostic logging to provided sinks:
    /// files, the console, and elsewhere (e.g. Graylog that we used in production environment).
    /// For details on how to set up the configuration for Serilog
    /// see https://procodeguide.com/programming/aspnet-core-logging-with-serilog/#Configure_Serilog_in_ASPNET_Core.
    /// </summary>
    private static void ConfigureLogger(IConfiguration configuration)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Destructure.UsingAttributes();

        loggerConfig.ConfigureAggregatedLogs(configuration);

        if (!AppEnvironment.IsDevelopment)
        {
            loggerConfig.Enrich.WithDockerContainerId();
            loggerConfig.ConfigureGraylog(configuration);
            //loggerConfig.ConfigureVictoriaLogslog(configuration);
            //loggerConfig.ConfigureOpenSearch(configuration, "Development");
            //loggerConfig.ConfigureQuickWit(configuration);
        }

        Log.Logger = loggerConfig.CreateLogger();
        ProgramLogger = Log.Logger.ForContext<Program>();

        if (AppEnvironment.IsDevelopment)
        {
            // Uses the console for output
            Serilog.Debugging.SelfLog.Enable(Console.Error);
        }
    }
}


