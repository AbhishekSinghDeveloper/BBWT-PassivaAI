using BBWM.AWS.EventBridge.Interfaces;
using BBWM.Core.Exceptions;

using BBWT.Tests.modules.BBWM.Core.Test.Extensions;

using Bogus;

using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace BBWM.AWS.EventBridge.Test;

public class AwsEventBridgeClientFactoryTests
{
    public static IEnumerable<object[]> InvalidSettingsTestData => new[]
    {
            new object[] { null },
            new object[] { ValidAwsSettings.Nullify(s => s.AwsRegion = null) },
            new object[] { ValidAwsSettings.Nullify(s => s.AwsRegion = string.Empty) },
            new object[] { ValidAwsSettings.Nullify(s => s.AccessKeyId = null) },
            new object[] { ValidAwsSettings.Nullify(s => s.AccessKeyId = string.Empty) },
            new object[] { ValidAwsSettings.Nullify(s => s.SecretAccessKey = null) },
            new object[] { ValidAwsSettings.Nullify(s => s.SecretAccessKey = string.Empty) },
        };

    [Theory]
    [MemberData(nameof(InvalidSettingsTestData))]
    public void CreateClient_Should_Throw_On_Invalid_Settings(AwsSettings awsSettings)
    {
        // Arrange
        var clientFactory = CreateClientFactory(awsSettings);

        // Act & Assert
        Assert.Throws<ConflictException>(clientFactory.CreateClient);
    }

    [Fact]
    public void CreateClient_Should_Create_Client()
    {
        // Arrange
        var clientFactory = CreateClientFactory(ValidAwsSettings);

        // Act
        var client = clientFactory.CreateClient();

        // Assert
        Assert.NotNull(client);
    }

    private static IAwsEventBridgeClientFactory CreateClientFactory(AwsSettings awsSettings)
    {
        var options = new Mock<IOptionsSnapshot<AwsSettings>>();
        options.SetupGet(s => s.Value).Returns(awsSettings);

        return new AwsEventBridgeClientFactory(options.Object);
    }

    private static readonly AwsSettings ValidAwsSettings =
        new Faker<AwsSettings>()
            .RuleFor(s => s.AccessKeyId, f => f.Random.AlphaNumeric(20).ToUpper())
            .RuleFor(s => s.SecretAccessKey, f => f.Random.AlphaNumeric(40))
            .RuleFor(s => s.AwsRegion, _ => "eu-west-1")
            .RuleFor(s => s.BucketName, _ => "aws-eb.unit.testing")
            .RuleFor(s => s.ParametersReloadingInterval, f => f.Random.Number(30, 60))
            .Generate();
}
