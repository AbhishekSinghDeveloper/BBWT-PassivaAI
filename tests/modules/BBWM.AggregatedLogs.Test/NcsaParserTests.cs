using Xunit;

namespace BBWM.AggregatedLogs.Test;

public class NcsaParserTests
{
    [Fact]
    public void ParseLine_Test()
    {
        var parser = new NcsaParser();

        var log = parser.Parse("::1 - user@something.com [31/May/2022:16:29:04 +0130] \"GET/api/getData/ HTTP/2\" 200 434", "BBWT3", "localhost", "IIS");

        Assert.NotNull(log);
        Assert.Equal("::1", log.IP);
        Assert.Equal("user@something.com", log.UserName);
        Assert.Equal(new DateTimeOffset(2022, 05, 31, 16, 29, 04, TimeSpan.FromMinutes(90)), log.TimeStamp);
        Assert.Equal(@"GET/api/getData/ HTTP/2", log.Message);
        Assert.Equal(200, log.HttpStatus);
        Assert.Equal("{\"Properties\":{\"ResponseSize\":434}}", log.LogEvent);
        Assert.Equal("BBWT3", log.AppName);
        Assert.Equal("IIS", log.Source);
        Assert.Equal("localhost", log.Server);
        Assert.Equal("Information", log.Level);
        Assert.Null(log.ErrorId);
        Assert.Null(log.OriginalUserName);
        Assert.Null(log.IsImpersonating);
        Assert.Null(log.Exception);

        log = parser.Parse("192:168:1:1 - - [31/May/2022:16:29:04 +0330] \"GET/api/getData/ HTTP/2\" 500 434", "BBWT3", "localhost", "IIS");
        Assert.NotNull(log);
        Assert.Equal("192:168:1:1", log.IP);
        Assert.Null(log.UserName);
        Assert.Equal("Error", log.Level);
        Assert.Null(log.ErrorId);
        Assert.Null(log.OriginalUserName);
        Assert.Null(log.IsImpersonating);
        Assert.Null(log.Exception);

        Assert.ThrowsAny<InvalidFormatException>(() => parser.Parse("::1 - user@something.com 31/May/2022:16:29:04 +0300] \"GET/api/getData/ HTTP/2\" 200 434", "BBWT3", "localhost", "IIS"));
        Assert.ThrowsAny<InvalidFormatException>(() => parser.Parse("::1 - user@something.com [41/May/2022:16:29:04 +0300] \"GET/api/getData/ HTTP/2\" 200 434", "BBWT3", "localhost", "IIS"));
    }
}
