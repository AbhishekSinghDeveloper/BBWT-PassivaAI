using BBWM.SystemSettings;

using Bogus;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using RichardSzalay.MockHttp;

using System.Net;
using System.Net.Mime;
using System.Text.Json;

using Xunit;

namespace BBWM.ReCaptcha.Test;

public class ReCaptchaServiceTests
{
    public ReCaptchaServiceTests()
    {
    }

    private static ReCaptchaAppSettings CreateReCaptchaAppSettings()
        => new Faker<ReCaptchaAppSettings>()
            .RuleFor(p => p.SiteKey, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.SecretKey, s => s.Random.AlphaNumeric(16))
            .RuleFor(p => p.ApiLink, s => s.Internet.Url())
            .RuleFor(p => p.AcceptableScore, s => s.Random.Decimal())
            .Generate();

    private static ReCaptchaService GetService(
        Action<MockHttpMessageHandler> configMessageHandler = default,
        ReCaptchaAppSettings reCaptchaAppSettings = default)
    {
        reCaptchaAppSettings ??= CreateReCaptchaAppSettings();
        var reCaptchaSettings = new ReCaptchaSettings
        {
            ValidateOnLoginEnabled = true,
        };

        var reCaptchaAppSettingsOptions = new Mock<IOptionsSnapshot<ReCaptchaAppSettings>>();
        reCaptchaAppSettingsOptions.Setup(p => p.Value).Returns(reCaptchaAppSettings);

        var settingsService = new Mock<ISettingsService>();
        settingsService.Setup(p => p.GetSettingsSection<ReCaptchaSettings>()).Returns(reCaptchaSettings);

        var messageHandler = new MockHttpMessageHandler();
        configMessageHandler?.Invoke(messageHandler);

        var http = new Mock<IHttpClientFactory>();
        http.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(messageHandler.ToHttpClient());

        var logger = new Mock<ILogger<ReCaptchaService>>();

        return new ReCaptchaService(
            reCaptchaAppSettingsOptions.Object, settingsService.Object, http.Object, logger.Object);
    }

    private static string GetReCaptchaResponse(decimal score, bool success = true)
        => JsonSerializer.Serialize(
            new Faker<CaptchaResponse>()
                .RuleFor(r => r.Score, _ => score)
                .RuleFor(r => r.Action, f => f.Random.Word())
                .RuleFor(r => r.Challenge_ts, f => f.Date.Recent())
                .RuleFor(r => r.Hostname, f => f.Internet.DomainName())
                .RuleFor(r => r.Success, _ => success)
                .Generate());

    [Fact]
    public async Task Should_Validate_reCaptcha_Token()
    {
        // Arrange
        var reCaptchaAppSettings = CreateReCaptchaAppSettings();
        const string ReCaptchaToken = "ResponseTestToken";
        var response = GetReCaptchaResponse(reCaptchaAppSettings.AcceptableScore + 1);

        var service = GetService(
            handler => handler
                .When(HttpMethod.Post, reCaptchaAppSettings.ApiLink)
                .WithQueryString("secret", reCaptchaAppSettings.SecretKey)
                .WithQueryString("response", ReCaptchaToken)
                .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, response),
            reCaptchaAppSettings);

        // Act
        var result = await service.CheckReCaptchaAsync(ReCaptchaToken);

        // Assert
        Assert.True(result);
    }
}
