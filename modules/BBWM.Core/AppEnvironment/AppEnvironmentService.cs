using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BBWM.Core.AppEnvironment;

public class AppEnvironmentService : IAppEnvironmentService
{
    private readonly IHostEnvironment hostEnvironment;
    private readonly IConfiguration configuration;

    public AppEnvironmentService(IHostEnvironment hostEnvironment, IConfiguration configuration)
    {
        this.hostEnvironment = hostEnvironment;
        this.configuration = configuration;
    }

    public bool IsLiveTypeEnvironment()
    {
        var envNames = configuration?.GetValue<string>("LiveTypeEnvironments")?
            .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (envNames is null || envNames.Length == 0)
        {
            envNames = AppEnvironment.DefaultLiveTypeEnvironments;
        }

        return envNames.Any(x => string.Equals(x, hostEnvironment.EnvironmentName, StringComparison.InvariantCultureIgnoreCase));
    }
}
