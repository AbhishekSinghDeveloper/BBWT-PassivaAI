using Microsoft.Extensions.Configuration;

namespace BBWM.Core.Web.Extensions;

public static class ConfigurationBuilderExtensions
{
    private const string EbConfigPath = @"C:\Program Files\Amazon\ElasticBeanstalk\config\containerconfiguration";

    public static IConfigurationBuilder AddEbConfig(this IConfigurationBuilder builder, out string environmentName)
    {
        // Parse and add EB Config environment variables' values
        var ebConfig = GetEbConfig();
        ebConfig.TryGetValue("ASPNETCORE_ENVIRONMENT", out environmentName);

        builder.AddInMemoryCollection(ebConfig);

        return builder;
    }

    private static Dictionary<string, string> GetEbConfig()
    {
        // Load EB Container Configuration as a separate IConfiguration instance so we don't mix it contents up with app configuration
        IConfiguration config = new ConfigurationBuilder().AddJsonFile(EbConfigPath, true, true).Build();
        return config.GetSection("iis:env").GetChildren().Select(pair => pair.Value.Split(new[] { '=' }, 2)).ToDictionary(keyPair => keyPair[0], keyPair => keyPair[1]);
    }
}
