using Bogus;

using Microsoft.Extensions.Options;

using Moq;

using RichardSzalay.MockHttp;

using System.Net;

namespace BBWM.GitLab.Test;

public class ServicesFactory
{
    private const string AwsApiUrl = "https://gitlab.bbconsult.co.uk";
    private const string GitLabApiUrl = "https://gitlab.bbconsult.co.uk";

    public static IGitLabService GetGitlabService(
        Action<MockHttpMessageHandler> configMessageHandler = default,
        GitLabSettings settings = default)
    {
        var messageHandler = new MockHttpMessageHandler();
        configMessageHandler?.Invoke(messageHandler);
        var client = messageHandler.ToHttpClient();

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        settings ??= GetGitLabSettingsFake().Generate();

        var mockGitLabSetting = new Mock<IOptionsSnapshot<GitLabSettings>>();
        mockGitLabSetting.Setup(p => p.Value).Returns(settings);

        return new GitLabService(mockGitLabSetting.Object, httpClientFactory.Object);
    }

    public static MockedRequest GitlabCommandOKStatusHandler(
            MockHttpMessageHandler handler,
            GitLabSettings settings,
            string command)
        => handler.When(HttpMethod.Post, $"{settings.GitLabApiUrl}/{command}")
            .With(ApiKeyHeader(settings.AwsApiToken))
            .Respond(HttpStatusCode.OK);

    private static Func<HttpRequestMessage, bool> ApiKeyHeader(string headerValue)
    {
        return (request) =>
        {
            var apiKeyHeader = request.Headers.GetValues("X-Api-Key");
            return apiKeyHeader.Count() == 1 && apiKeyHeader.First() == headerValue;
        };
    }

    public static Faker<GitLabSettings> GetGitLabSettingsFake()
        => new Faker<GitLabSettings>()
            .RuleFor(p => p.ProjectId, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.AwsApiToken, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.AwsApiUrl, s => AwsApiUrl)
            .RuleFor(p => p.Branch, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.GitLabApiUrl, s => GitLabApiUrl)
            .RuleFor(p => p.GitLabApiToken, s => s.Random.AlphaNumeric(7));
}
