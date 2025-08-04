using AutoMapper;
using BBWM.Core.Membership.Constants;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Test.Fixtures;
using BBWM.Core.Test.Utils;
using BBWT.Data;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace BBWM.Core.Membership.Test.Services;

public class LoginAuditServiceTests : IClassFixture<MappingFixture>
{
    private static readonly Random random = new();

    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 Edg/96.0.1054.62";

    public LoginAuditServiceTests(MappingFixture mappingFixture)
        => Mapper = mappingFixture.DefaultMapper;

    public IMapper Mapper { get; }

    [Fact]
    public async Task GetLastAttemptsCountAsync()
    {
        // Arrange
        List<LoginAudit> loginAudits = Mapper.Map<List<LoginAudit>>(CreateLoginAudits(100));
        using IDataContext dataContext =
            await SutDataHelper.CreateContextWithData<IDataContext, LoginAudit>(loginAudits.ToArray());
        var ipInfo = loginAudits
            .GroupBy(l => l.Ip)
            .Select(g => new { Ip = g.Key, Count = g.Count() })
            .OrderBy(i => i.Count)
            .FirstOrDefault();

        LoginAuditService loginAuditService = new(
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<ILogger<LoginAuditService>>(),
            dataContext);

        // Act
        int count = await loginAuditService.GetLastAttemptsCountAsync(ipInfo.Ip, DateTimeOffset.MinValue);

        // Assert
        Assert.Equal(ipInfo.Count, count);
    }

    [Fact]
    public async Task GetLastSuccessfulLoginAuditAsync()
    {
        // Arrange
        List<LoginAudit> loginAudits = Mapper.Map<List<LoginAudit>>(CreateLoginAudits(100));
        List<LoginAudit> choosenAudits =
            Enumerable.Range(1, 10).Select(i => random.Next(0, 100)).Select(i => loginAudits[i]).ToList();
        const string Email = "some_xyz_123@email.com";
        choosenAudits.ForEach(l =>
        {
            l.Email = Email;
            l.Result = LogMessages.LoggedIn;
        });
        LoginAudit expectedAudit = choosenAudits.OrderByDescending(a => a.Datetime).First();

        using IDataContext dataContext =
            await SutDataHelper.CreateContextWithData<IDataContext, LoginAudit>(loginAudits.ToArray());

        LoginAuditService loginAuditService = new(
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<ILogger<LoginAuditService>>(),
            dataContext);

        // Act
        LoginAudit audit = await loginAuditService.GetLastSuccessfulLoginAuditAsync(Email);

        // Assert
        Assert.Equal(expectedAudit.Id, audit.Id);
    }

    [Theory]
    [MemberData(nameof(SaveLoginAuditTestData))]
    public async Task SaveLoginAuditAsync_Should_Save_Audit_Login(
        string browserId,
        string fingerprint,
        string realIp,
        string userAgent,
        IPAddress connIp,
        User user,
        string expectedBrowserId,
        string expectedIp)
    {
        // Arrange
        HttpContext httpContext = new DefaultHttpContext();
        if (!string.IsNullOrEmpty(browserId))
            httpContext.Request.Headers["X-Browser-Id"] = browserId;
        httpContext.Request.Headers["X-Browser-Fingerprint"] = fingerprint;
        if (!string.IsNullOrEmpty(userAgent))
            httpContext.Request.Headers["User-Agent"] = userAgent;
        if (!string.IsNullOrEmpty(realIp))
            httpContext.Request.Headers["X-Real-IP"] = realIp;
        httpContext.Connection.RemoteIpAddress = connIp;

        Mock<IHttpContextAccessor> contextAccessor = new();
        contextAccessor.Setup(ca => ca.HttpContext).Returns(httpContext);

        IDataContext dataContext = SutDataHelper.CreateEmptyContext<IDataContext>();

        LoginAuditService loginAuditService =
            new(contextAccessor.Object, Mock.Of<ILogger<LoginAuditService>>(), dataContext);

        const string Result = "XYZ789";

        // Act
        await loginAuditService.SaveLoginAuditAsync(user, Result);

        // Assert
        LoginAudit loginAudit = Assert.Single(dataContext.Set<LoginAudit>());
        if (expectedBrowserId is not null)
            Assert.Equal(expectedBrowserId, loginAudit.Browser);
        Assert.Equal(expectedIp, loginAudit.Ip);
        Assert.Equal(fingerprint, loginAudit.Fingerprint);
        Assert.Equal(Result, loginAudit.Result);
    }

    public static IEnumerable<object[]> SaveLoginAuditTestData => new[]
    {
            new object[]
            {
                "Mozilla-v123",         // browserId
                "0123456789",           // fingerprint
                "198.162.1.1",          // realIp
                null,                   // userAgent
                null,                   // connIp
                null,                   // user
                "Mozilla-v123",         // expectedBrowserId
                "198.162.1.1",          // expectedIp
            },
            new object[]
            {
                null,
                "0123456789",
                null,
                UserAgent,
                IPAddress.Parse("198.162.1.1"),
                new User { Id = $"{Guid.NewGuid():N}", Email = "some@email.com" },
                "Edge 96.0.1054",
                "198.162.1.1",
            },
            new object[]
            {
                null,
                "0123456789",
                null,
                "abc",
                IPAddress.Parse("198.162.1.1"),
                null,
                null,
                "198.162.1.1",
            },
        };

    private List<LoginAuditDTO> CreateLoginAudits(int count = 10)
        => new Faker<LoginAuditDTO>()
            .RuleFor(l => l.Browser, f => f.Random.AlphaNumeric(10))
            .RuleFor(l => l.Datetime, _ => RandomDateTime())
            .RuleFor(l => l.Email, f => $"f_{f.Internet.Email()}")
            .RuleFor(l => l.Fingerprint, f => f.Random.AlphaNumeric(16))
            .RuleFor(l => l.Ip, f => f.Internet.Ip())
            .RuleFor(l => l.Result, f => f.Lorem.Sentence())
            .Generate(count);

    private static DateTimeOffset RandomDateTime()
    {
        long randomTicks;
        do
        {
            byte[] bytes = new byte[8];
            random.NextBytes(bytes);
            randomTicks = BitConverter.ToInt64(bytes, 0);
        }
        while (randomTicks < DateTimeOffset.MinValue.Ticks || randomTicks > DateTimeOffset.MaxValue.Ticks);

        return new DateTimeOffset(randomTicks, TimeSpan.Zero);
    }
}
