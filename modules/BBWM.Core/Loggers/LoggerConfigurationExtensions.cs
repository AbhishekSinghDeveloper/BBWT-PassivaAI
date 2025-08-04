using BBWM.Core.Data;
using BBWM.Core.Loggers.VictoriaLogs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NpgsqlTypes;
using OpenSearch.Client;
using OpenSearch.Net;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Graylog.Batching;
using Serilog.Sinks.Graylog.Core.Transport;
using Serilog.Sinks.MariaDB.Extensions;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.OpenSearch;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;

namespace BBWM.Core.Loggers;

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration ConfigureGraylog(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        var graylogSettings = configuration.GetSection("Graylog").Get<GraylogSettings>();

        if (graylogSettings?.Enabled ?? false && !string.IsNullOrWhiteSpace(graylogSettings?.ProjectName))
        {
            loggerConfiguration = loggerConfiguration.WriteTo.Graylog(new BatchingGraylogSinkOptions
            {
                HostnameOrAddress = "http://graylog.bbconsult.co.uk",
                Port = 12201,
                TransportType = TransportType.Http,
                //PeriodicOptions = new Serilog.Sinks.PeriodicBatching.PeriodicBatchingSinkOptions { QueueLimit = 1000},
                QueueLimit = 1000,
                MinimumLogEventLevel = GraylogSettings.ParseLogEventLevel(graylogSettings.LogEventLevel, LogEventLevel.Warning)
            });

            Serilog.Context.LogContext.PushProperty("ProjectName", graylogSettings.ProjectName);
        }

        return loggerConfiguration;
    }

    /*public static LoggerConfiguration ConfigureVictoriaLogslog(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        var victoriaLogsSettings = configuration.GetSection("VictoriaLogs").Get<GraylogSettings>();

        if (victoriaLogsSettings?.Enabled ?? false && !string.IsNullOrWhiteSpace(victoriaLogsSettings?.ProjectName))
        {
            loggerConfiguration = loggerConfiguration.WriteTo.VictoriaLogsSink(
                victoriaLogsSettings.ProjectName
            //{
            //    HostnameOrAddress = "http://127.0.0.1",
            //    Port = 9428,
            //    TransportType = TransportType.Http,
            //    QueueLimit = 1000,
            //    MinimumLogEventLevel = GraylogSettings.ParseLogEventLevel(victoriaLogsSettings.LogEventLevel, LogEventLevel.Warning)
            //}
    );
        }

        return loggerConfiguration;
    }*/

    public static LoggerConfiguration ConfigureQuickWit(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        var settings = configuration.GetSection("QuickWit").Get<QuickWitSettings>();

        if (settings?.Enabled ?? false && !string.IsNullOrWhiteSpace(settings.ProjectName))
        {
            loggerConfiguration = loggerConfiguration.WriteTo.QuickWitSink(
                settings.ProjectName,
                new QuickWit.QuickWitOptions
                {
                    Hostname = settings.HostName,
                    SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { new StringEnumConverter()}
                    },
                    MinimumLogEventLevel = QuickWitSettings.ParseLogEventLevel(settings.LogEventLevel, LogEventLevel.Warning)
                }
            );
        }

        return loggerConfiguration;
    }

    //public static LoggerConfiguration ConfigureOpenSearch(
    //    this LoggerConfiguration loggerConfiguration,
    //    IConfiguration configuration,
    //    string environment)
    //{
    //    //var graylogSettings = configuration.GetSection("OpenSearch").Get<GraylogSettings>();

    //    //if (graylogSettings?.Enabled ?? false && !string.IsNullOrWhiteSpace(graylogSettings?.ProjectName))
    //   // {
    //        loggerConfiguration = loggerConfiguration
    //            .WriteTo
    //            .OpenSearch(new OpenSearchSinkOptions(
    //            new Uri("http://localhost:9200"))
    //        {

    //            QueueSizeLimit = 1000,
    //                IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
    //                AutoRegisterTemplate = true,
    //            });

    //      //  Serilog.Context.LogContext.PushProperty("ProjectName", graylogSettings.ProjectName);
    //    //}

    //    return loggerConfiguration;
    //}

    public static LoggerConfiguration ConfigureAggregatedLogs(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        var aggregatedLogsSettings = configuration.GetSection(AggregatedLogsSettings.AggregatedLogsSettingsDefaultSectionName).Get<AggregatedLogsSettings>()
                ?? new AggregatedLogsSettings();

        if (aggregatedLogsSettings.Enabled)
        {
            var minimumLevel = aggregatedLogsSettings.LogEventLevel.ParseLogEventLevel(LogEventLevel.Information);

            switch (aggregatedLogsSettings.DatabaseType)
            {
                case DatabaseType.MsSql:
                    loggerConfiguration = loggerConfiguration.WriteTo.MSSqlServer(
                        connectionString: aggregatedLogsSettings.ConnectionString,
                        sinkOptions: new MSSqlServerSinkOptions
                        {
                            TableName = AggregatedLogsSettings.TableName,
                            BatchPeriod = TimeSpan.FromSeconds(aggregatedLogsSettings.Period),
                            BatchPostingLimit = aggregatedLogsSettings.BatchLimit
                        },
                        columnOptions: CreateMsSqlColumnOptions()
                    );
                    break;
                     
                case DatabaseType.MySql:
                    loggerConfiguration = loggerConfiguration.WriteTo.MariaDB(
                        connectionString: aggregatedLogsSettings.ConnectionString,
                        tableName: AggregatedLogsSettings.TableName,
                        restrictedToMinimumLevel: minimumLevel,
                        options: new Serilog.Sinks.MariaDB.MariaDBSinkOptions
                        {
                            ExcludePropertiesWithDedicatedColumn = true,
                            PropertiesToColumnsMapping = CreateMySqlColumnOptions()
                        },
                        period: TimeSpan.FromSeconds(aggregatedLogsSettings.Period),
                        batchPostingLimit: aggregatedLogsSettings.BatchLimit
                    );
                    break;

                case DatabaseType.PostgreSql:
                    loggerConfiguration = loggerConfiguration.WriteTo.PostgreSQL(
                        connectionString: aggregatedLogsSettings.ConnectionString,
                        tableName: AggregatedLogsSettings.TableName,
                        restrictedToMinimumLevel: minimumLevel,
                        columnOptions: CreatePostgreSqlColumnOptions(),
                        period: TimeSpan.FromSeconds(aggregatedLogsSettings.Period),
                        batchSizeLimit: aggregatedLogsSettings.BatchLimit
                    );
                    break;

                default: throw new Exception($"Data base type '{aggregatedLogsSettings.DatabaseType}' is not supported.");
            };

            loggerConfiguration
                .Enrich.With<SourceEnricher>()
                .Enrich.With<ServerNameEnricher>();
        }

        return loggerConfiguration;
    }

    private static Serilog.Sinks.MSSqlServer.ColumnOptions CreateMsSqlColumnOptions()
    {
        var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
        columnOptions.Store.Remove(StandardColumn.Properties);
        columnOptions.Store.Remove(StandardColumn.MessageTemplate);
        columnOptions.Store.Add(StandardColumn.LogEvent);
        columnOptions.TimeStamp.DataType = SqlDbType.DateTimeOffset;
        columnOptions.LogEvent.ExcludeStandardColumns = true;
        columnOptions.LogEvent.ExcludeAdditionalProperties = true;
        columnOptions.AdditionalColumns = new Collection<SqlColumn>
        {
            new SqlColumn {ColumnName = "AppName", PropertyName = "AppName", DataType = SqlDbType.NVarChar, DataLength = 64},
            new SqlColumn {ColumnName = "Source", PropertyName = "Source", DataType = SqlDbType.NVarChar, DataLength = 64},
            new SqlColumn {ColumnName = "UserName", PropertyName = "Username", DataType = SqlDbType.NVarChar, DataLength = 64},
            new SqlColumn {ColumnName = "IsImpersonating", PropertyName = "IsImpersonating", DataType = SqlDbType.Bit},
            new SqlColumn {ColumnName = "OriginalUserName", PropertyName = "OriginalUserName", DataType = SqlDbType.NVarChar, DataLength = 64},
            new SqlColumn {ColumnName = "IP", PropertyName = "IP", DataType = SqlDbType.NVarChar, DataLength = 64},
            new SqlColumn {ColumnName = "Server", PropertyName = "Server", DataType = SqlDbType.NVarChar, DataLength = 64},
            new SqlColumn {ColumnName = "ErrorId", PropertyName = "ErrorId", DataType = SqlDbType.NVarChar, DataLength = 64},
            new SqlColumn {ColumnName = "HttpStatus", PropertyName = "HttpStatus", DataType = SqlDbType.Int}
        };
        return columnOptions;
    }

    private static Dictionary<string, ColumnWriterBase> CreatePostgreSqlColumnOptions() =>
        new Dictionary<string, ColumnWriterBase>
        {
            {"Id", new IdAutoIncrementColumnWriter() },
            {"Message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
            {"Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
            {"TimeStamp", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
            {"Exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
            {"LogEvent", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
            {"AppName", new SinglePropertyColumnWriter("AppName", PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
            {"Source", new SinglePropertyColumnWriter("Source", PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
            {"UserName", new SinglePropertyColumnWriter("Username", PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
            {"IsImpersonating", new SinglePropertyColumnWriter("IsImpersonating", PropertyWriteMethod.Raw, NpgsqlDbType.Boolean) },
            {"OriginalUserName", new SinglePropertyColumnWriter("OriginalUserName", PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
            {"IP", new SinglePropertyColumnWriter("IP", PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
            {"Server", new SinglePropertyColumnWriter("Server", PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
            {"ErrorId", new SinglePropertyColumnWriter("ErrorId", PropertyWriteMethod.ToString, NpgsqlDbType.Text) },
            {"HttpStatus", new SinglePropertyColumnWriter("HttpStatus", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) }
        };

    private static Dictionary<string, string> CreateMySqlColumnOptions() =>
        new Dictionary<string, string>
        {
            ["Message"] = "Message",
            ["Level"] = "Level",
            ["TimeStamp"] = "TimeStamp",
            ["Exception"] = "Exception",
            ["Properties"] = "LogEvent",
            ["AppName"] = "AppName",
            ["Source"] = "Source",
            ["UserName"] = "UserName",
            ["IsImpersonating"] = "IsImpersonating",
            ["OriginalUserName"] = "OriginalUserName",
            ["IP"] = "IP",
            ["Server"] = "Server",
            ["ErrorId"] = "ErrorId",
            ["HttpStatus"] = "HttpStatus"
        };

    public static LoggerConfiguration WithDockerContainerId(this LoggerEnrichmentConfiguration config) =>
        config.With<DockerContainerIdEnricher>();

    public static LogEventLevel ParseLogEventLevel(this string s, LogEventLevel defaultLevel) =>
        (s?.ToLower()) switch
        {
            "verbose" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => defaultLevel,
        };
}
