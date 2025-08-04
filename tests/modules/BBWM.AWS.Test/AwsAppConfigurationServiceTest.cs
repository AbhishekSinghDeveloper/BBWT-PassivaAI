using Bogus;

using Microsoft.Extensions.Options;

using Moq;

using Xunit;

using Parameter = BBWM.AppConfiguration.Parameter;

namespace BBWM.AWS.Test;

public class AwsAppConfigurationServiceTest
{
    public AwsAppConfigurationServiceTest()
    {
    }

    private static AwsAppConfigurationService GetService()
    {
        var awsSettings = new Faker<AwsSettings>()
            .RuleFor(p => p.AccessKeyId, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.SecretAccessKey, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.ParametersPath, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.ParametersReloadingInterval, s => s.Random.Int())
            .RuleFor(p => p.AwsRegion, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.BucketName, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.S3Url, s => s.Random.AlphaNumeric(7))
            .Generate();

        var mock = new Mock<IOptionsSnapshot<AwsSettings>>();
        mock.Setup(p => p.Value).Returns(awsSettings);

        return new AwsAppConfigurationService(mock.Object);
    }

    [Fact]
    public async Task Get_All_Test()
    {
        var service = GetService();
        var result = service.GetAll();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Get_By_Name_Test()
    {
        var service = GetService();
        var result = service.GetByName("test");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Put_Test()
    {
        var service = GetService();

        var fakeParam = new Faker<Parameter>();
        fakeParam.RuleFor(p => p.Name, s => s.Random.AlphaNumeric(7));
        fakeParam.RuleFor(p => p.Value, s => s.Random.AlphaNumeric(7));
        fakeParam.RuleFor(p => p.Secure, s => s.Random.Bool());

        var result = service.Put(fakeParam.Generate());

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Delete_Test()
    {
        var service = GetService();

        var result = service.Delete("test");

        Assert.NotNull(result);
    }
}
