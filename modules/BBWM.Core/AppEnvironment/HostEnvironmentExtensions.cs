using Microsoft.Extensions.Hosting;

namespace BBWM.Core.AppEnvironment;

/// <summary>
/// Extends default <see cref="HostEnvironmentEnvExtensions"/> with BBWT-specific environments.
/// 
/// Note! A customer project shouldn't extend environments list in BBWM.Core, instead
/// you can add it into the template (BBWT.*) project part. See BBWT.Server.Extensions.HostEnvironmentExtensions.
/// </summary>
public static class HostEnvironmentExtensions
{
    /// <summary>
    /// Checks if the current host environment name is <see cref="AppEnvironmentNames.Test"/>.
    /// </summary>
    /// <param name="hostEnvironment">An instance of <see cref="IHostEnvironment"/>.</param>
    /// <returns>True if the environment name is <see cref="EnvironmentName.Test"/>, otherwise false.</returns>
    public static bool IsTest(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment == null)
        {
            throw new ArgumentNullException(nameof(hostEnvironment));
        }

        return hostEnvironment.IsEnvironment(AppEnvironmentNames.Test);
    }

    /// <summary>
    /// Checks if the current host environment name is <see cref="AppEnvironmentNames.UAT"/>.
    /// </summary>
    /// <param name="hostEnvironment">An instance of <see cref="IHostEnvironment"/>.</param>
    /// <returns>True if the environment name is <see cref="AppEnvironmentNames..UAT"/>, otherwise false.</returns>
    public static bool IsUAT(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment == null)
        {
            throw new ArgumentNullException(nameof(hostEnvironment));
        }

        return hostEnvironment.IsEnvironment(AppEnvironmentNames.UAT);
    }
}