using BBWM.AggregatedLogs.Lambda;
using BBWM.AggregatedLogs.Lambda.DTO;
using BBWM.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace BBWM.AggregatedLogs.Test;

public class AmazonLambdaLogParserTests
{
    [Fact]
    public async Task ParseAll_Test()
    {
        var parser = new AmazonLambdaLogParser();

        var contextOptions = new DbContextOptionsBuilder<LogContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), new InMemoryDatabaseRoot())
                .Options;

        var logContext = new LogContext(contextOptions);

        var dto = new EventDTO
        {
            awslogs = new AwsLogDTO
            {
                data = "H4sIAAAAAAAAAI2RwW7bMBBEf6UgeowickUtSd8E1MnFPUmnxEZAmStBgC25FN00MPzvWTkt0BwK9EYOh5x9w4s40jz7npq3E4mV+FY11cv3dV1Xj2txJ6bXkSLLRsoSUFmptGb5MPWPcTqf+CT3r3N+8Mc2+Hwz9fV0jntqaE6bm/ZhrlMkf2Q3SIBcYi5t/vx1UzXrutkpC8EFxM6D12iCt94UBqTek6fgW35iPrfzPg6nNEzjw3BIFGexehZV30fqfaLAyXP9l0nsbrnrnzSmxXoRQ+D4Ap2UViECFFgYXWIJztkCjTZaK1U47SwoBnU8KBgwJSoAySOkgYtK/sjMCkuN1lldgrV3fwr8TZdJzKRtFKykXsnyni1P21Ra2zmkImuho0wX1GXeQMgIO2gBnOo0btMwdtM2LeV9uS1HzqVfKfo9Iz4MdAjMchHp46sWy2JYGHn7+VqkH2cWXm7Y/5P+GfGfKOJ63V3fAX9ZuAY1AgAA",
            },
        };

        var service = new LambdaLogService(logContext, parser);

        await service.ProcessLogs(dto);

        Assert.Single(logContext.Logs);
    }

    [Fact]
    public async Task ParseLine_Test()
    {
        var parser = new AmazonLambdaLogParser();

        var logs = await parser.Parse(new EventDTO
        {
            awslogs = new AwsLogDTO
            {
                data = "H4sIAAAAAAAAAI2RwW7bMBBEf6UgeowickUtSd8E1MnFPUmnxEZAmStBgC25FN00MPzvWTkt0BwK9EYOh5x9w4s40jz7npq3E4mV+FY11cv3dV1Xj2txJ6bXkSLLRsoSUFmptGb5MPWPcTqf+CT3r3N+8Mc2+Hwz9fV0jntqaE6bm/ZhrlMkf2Q3SIBcYi5t/vx1UzXrutkpC8EFxM6D12iCt94UBqTek6fgW35iPrfzPg6nNEzjw3BIFGexehZV30fqfaLAyXP9l0nsbrnrnzSmxXoRQ+D4Ap2UViECFFgYXWIJztkCjTZaK1U47SwoBnU8KBgwJSoAySOkgYtK/sjMCkuN1lldgrV3fwr8TZdJzKRtFKykXsnyni1P21Ra2zmkImuho0wX1GXeQMgIO2gBnOo0btMwdtM2LeV9uS1HzqVfKfo9Iz4MdAjMchHp46sWy2JYGHn7+VqkH2cWXm7Y/5P+GfGfKOJ63V3fAX9ZuAY1AgAA"
            }
        });

        Assert.Single(logs);

        var log = logs.First();

        Assert.Equal("Information", log.Level);
        Assert.Equal("/aws/lambda/LogSourceTestLambda", log.AppName);
        Assert.Equal("CloudWatch", log.Source);
        Assert.Equal("Test info\n", log.Message);
        Assert.Equal(new DateTimeOffset(2022, 6, 8, 12, 4, 5, 288, TimeSpan.Zero), log.TimeStamp);
        Assert.Equal("{\"Properties\":{\"RequestId\":588f96e3-b2fe-43ef-a72d-e6f2b2291f46}}", log.LogEvent);
        Assert.Null(log.ErrorId);
        Assert.Null(log.OriginalUserName);
        Assert.Null(log.IsImpersonating);
        Assert.Null(log.Exception);
        Assert.Null(log.Server);
        Assert.Null(log.IP);
        Assert.Null(log.UserName);
        Assert.Null(log.HttpStatus);
    }

    [Fact]
    public async Task TestDefaultSettings()
    {
        var settings = new LogLambdaDatabaseSettings();

        Assert.Equal(10, settings.MaxRetryCount);
        Assert.Equal(30, settings.MaxRetryDelay);
        Assert.Equal(DatabaseType.MySql, settings.DatabaseType);
        Assert.Null(settings.ErrorNumbersToAdd);
        Assert.Null(settings.ConnectionString);
    }
}
