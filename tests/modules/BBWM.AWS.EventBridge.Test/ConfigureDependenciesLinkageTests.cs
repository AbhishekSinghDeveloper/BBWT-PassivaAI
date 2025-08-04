using BBWM.Core.Exceptions;

using Bogus;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

using System.Reflection;

using Xunit;

namespace BBWM.AWS.EventBridge.Test;

public enum InvalidPropertyValueType
{
    Empty = 1,
    Null,
}

public class MyServiceCollection : ServiceCollection, IServiceCollection
{
    private bool verified = false;

    void ICollection<ServiceDescriptor>.Add(ServiceDescriptor item)
    {
        if (item.ServiceType == typeof(IOptionsChangeTokenSource<AwsEventBridgeSettings>))
            verified = true;
    }

    public void Verify()
    {
        if (!verified)
            throw new Exception("Settings not registered");
    }
}

public class ConfigureDependenciesLinkageTests
{
    public static readonly List<object[]> RequiredProperties = new()
    {
        new[] { nameof(AwsEventBridgeSettings.APIKey) },
        new[] { nameof(AwsEventBridgeSettings.TargetRoleArn) },
        new[] { nameof(AwsEventBridgeSettings.ApiConnectionName) },
        new[] { nameof(AwsEventBridgeSettings.ApiDestinationName) },
    };

    private static AwsEventBridgeSettings CreateSettings()
        => new Faker<AwsEventBridgeSettings>()
            .RuleFor(s => s.AuthHeader, f => f.Random.AlphaNumeric(10))
            .RuleFor(s => s.APIKey, f => f.Random.AlphaNumeric(18))
            .RuleFor(s => s.ApiConnectionName, f => f.Random.AlphaNumeric(7))
            .RuleFor(s => s.ApiDestinationName, f => f.Random.AlphaNumeric(7))
            .RuleFor(s => s.TargetRoleArn, f => f.Random.AlphaNumeric(15))
            .Generate();

    private static AwsEventBridgeSettings CreateInvalidSettings(string invalidProperty, InvalidPropertyValueType valueType)
    {
        var settings = CreateSettings();

        var newValue = valueType switch
        {
            InvalidPropertyValueType.Empty => string.Empty,
            InvalidPropertyValueType.Null => null,
            _ => "-1"
        };

        typeof(AwsEventBridgeSettings).GetProperty(invalidProperty).SetValue(settings, newValue);

        return settings;
    }

    private static IEnumerable<KeyValuePair<string, string>> ExtractSettings(AwsEventBridgeSettings settings)
    {
        foreach (var prop in typeof(AwsEventBridgeSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            yield return new($"{AwsEventBridgeSettings.CONFIG_SECTION}:{prop.Name}", prop.GetValue(settings)?.ToString());
        }
    }

    [Theory]
    [MemberData(nameof(RequiredProperties), MemberType = typeof(ConfigureDependenciesLinkageTests))]
    public void ConfigureServices_Should_Throw_Conflict(string property)
    {
        foreach (var valueType in new[] { InvalidPropertyValueType.Empty, InvalidPropertyValueType.Null })
        {
            // Arrange
            var settings = CreateInvalidSettings(property, valueType);
            var config = new ConfigurationBuilder().AddInMemoryCollection(ExtractSettings(settings)).Build();
            var services = Mock.Of<IServiceCollection>();

            // Act
            var linkage = new ConfigureDependenciesLinkage();

            // Assert
            Assert.Throws<ConflictException>(() => linkage.ConfigureServices(services, config));
        }
    }

    [Fact]
    public void ConfigureServices_Should_Throw_Conflict_On_Missing_Settings()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var services = Mock.Of<IServiceCollection>();

        // Act
        var linkage = new ConfigureDependenciesLinkage();

        // Assert
        Assert.Throws<ConflictException>(() => linkage.ConfigureServices(services, config));
    }

    [Fact]
    public void ConfigureServices_Should_Configure_Settings()
    {
        // Arrange
        var settings = CreateSettings();
        var config = new ConfigurationBuilder().AddInMemoryCollection(ExtractSettings(settings)).Build();
        var services = new MyServiceCollection();

        // Act
        var linkage = new ConfigureDependenciesLinkage();

        // Assert
        linkage.ConfigureServices(services, config);
        services.Verify();
    }
}
