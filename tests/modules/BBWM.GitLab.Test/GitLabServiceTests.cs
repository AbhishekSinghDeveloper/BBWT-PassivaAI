using Xunit;

namespace BBWM.GitLab.Test;

public class GitLabServiceTests
{
    public GitLabServiceTests()
    {
    }

    [Fact]
    public async Task Push_Test()
    {
        // Arrange
        var settings = ServicesFactory.GetGitLabSettingsFake().Generate();

        var service = ServicesFactory.GetGitlabService(
                handler => ServicesFactory.GitlabCommandOKStatusHandler(handler, settings, "gitpush"),
                settings);

        // Act
        var pushed = await service.Push("testFunction", "testContent", "testUserName", CancellationToken.None);

        // Assert
        Assert.True(pushed);
    }

    [Fact]
    public async Task Run_Test()
    {
        // Arrange
        var settings = ServicesFactory.GetGitLabSettingsFake().Generate();

        var service = ServicesFactory.GetGitlabService(
            handler => ServicesFactory.GitlabCommandOKStatusHandler(handler, settings, "testFunction"),
                settings);

        // Act
        var run = await service.Run("testFunction", "testContent", "testUserName", CancellationToken.None);

        // Assert
        Assert.True(run);
    }
}
