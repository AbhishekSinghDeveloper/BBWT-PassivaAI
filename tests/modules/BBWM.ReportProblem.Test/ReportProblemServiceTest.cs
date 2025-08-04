using BBWM.Messages;
using BBWM.SystemData;

using Bogus;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace BBWM.ReportProblem.Test;

public class ReportProblemServiceTest
{
    private static Randomizer randomizer = new Randomizer();
    private static string userAgent = randomizer.String();
    private static string baseURL = randomizer.String();

    private static (ReportProblemService, Mock<IEmailSender>) GetService(SupportSettings supportSettings = default)
    {
        supportSettings ??= new Faker<SupportSettings>()
            .RuleFor(s => s.EmailAddress1, f => f.Internet.Email())
            .RuleFor(s => s.EmailAddress2, f => f.Internet.Email())
            .RuleFor(s => s.EmailAddress3, f => f.Internet.Email())
            .RuleFor(s => s.EmailAddress4, f => f.Internet.Email())
            .Generate();

        var emailSettings = new Faker<EmailSettings>()
            .RuleFor(s => s.FromAddress, f => f.Internet.Email())
            .Generate();

        var emailSender = new Mock<IEmailSender>();
        emailSender
            .Setup(e => e.SendEmail(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IFormFile[]>(),
                It.IsAny<EmailBrandInfo>(), supportSettings.EmailAddress1, supportSettings.EmailAddress2,
                supportSettings.EmailAddress3, supportSettings.EmailAddress4))
            .Verifiable();

        var supportSettingsOptions = new Mock<IOptionsSnapshot<SupportSettings>>();
        supportSettingsOptions.Setup(o => o.Value).Returns(supportSettings);

        var emailSettingsOptions = new Mock<IOptionsSnapshot<EmailSettings>>();
        emailSettingsOptions.Setup(o => o.Value).Returns(emailSettings);

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(c => c.HttpContext).Returns(new DefaultHttpContext());

        var service = new ReportProblemService(
            emailSender.Object,
            supportSettingsOptions.Object,
            emailSettingsOptions.Object,
            httpContextAccessor.Object,
            Mock.Of<ISystemDataService>());

        return (service, emailSender);
    }

    #region Send method

    [Fact]
    public async Task Send_Should_Send_Report()
    {
        // Arrange
        var report = GetEntity();
        var (service, emailSender) = GetService();

        // Act
        await service.Send(report, userAgent, baseURL);

        // Assert
        emailSender.Verify();
    }
    #endregion

    #region AutoSend methods

    [Fact]
    public async void AutoSend_Should_Not_Send_Null_Exception()
    {
        // Arrange
        var (service, emailSender) = GetService();

        // Act
        await service.AutoSend((Exception)null);

        // Assert
        emailSender.Verify(
            s => s.SendEmail(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IFormFile[]>(), It.IsAny<EmailBrandInfo>(), It.IsAny<string[]>()),
            Times.Never());
    }

    [Fact]
    public async void AutoSend_Should_Not_Send_Null_ErrorLog()
    {
        // Arrange
        var (service, emailSender) = GetService();

        // Act
        await service.AutoSend((ErrorLogDTO)null);

        // Assert
        emailSender.Verify(
            s => s.SendEmail(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IFormFile[]>(), It.IsAny<EmailBrandInfo>(), It.IsAny<string[]>()),
            Times.Never());
    }

    [Fact]
    public async Task AutoSend_Should_Send_Exception()
    {
        // Arrange
        var (service, emailSender) = GetService();

        // Act
        await service.AutoSend(new Exception("Something terribly happened!"));

        // Assert
        emailSender.Verify();
    }

    [Fact]
    public async Task AutoSend_Should_Send_ErrorLog()
    {
        // Arrange
        var (service, emailSender) = GetService();
        var errorLog = GetErrorLogEntity();

        // Act
        await service.AutoSend(errorLog);

        // Assert
        emailSender.Verify();
    }

    #endregion

    protected static ReportProblemDTO GetEntity()
    {
        var faker = new Faker<ReportProblemDTO>()
            .RuleFor(p => p.Description, s => s.Random.Words())
            .RuleFor(p => p.Email, s => s.Person.Email)
            .RuleFor(p => p.ErrorLog, s => Enumerable.Range(1, 5).Select(_ => new Faker().Random.Words()).ToArray())
            .RuleFor(p => p.Severity, s => s.Random.String())
            .RuleFor(p => p.Subject, s => s.Random.String())
            .RuleFor(p => p.Time, s => s.Date.Random.ToString())
            .RuleFor(p => p.User, s => s.Random.String());
        return faker.Generate();
    }

    protected static ErrorLogDTO GetErrorLogEntity()
    {
        var faker = new Faker<ErrorLogDTO>()
            .RuleFor(p => p.ExceptionMessage, s => s.Random.Words())
            .RuleFor(p => p.ExceptionType, s => "Exception")
            .RuleFor(p => p.Path, s => s.Address.Locale)
            .RuleFor(p => p.PathBase, s => s.Address.Locale)
            .RuleFor(p => p.StackTrace, s => s.Internet.Locale);
        return faker.Generate();
    }
}
