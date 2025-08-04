namespace BBWT.Server.Extensions;

/// <summary>
/// Custom application environment names, specific for this customer project deployment workflow
/// </summary>
public static class BbwtAppEnvironments
{
    //public static readonly string SomeProjectSpecificEnvName = "SomeEnvName";
}

/// <summary>
/// Extends default <see cref="BBWM.Core.HostEnvironmentExtensions"/> with project-specific environments.
/// </summary>
public static class HostEnvironmentExtensions
{
    /// An example of checking that current environment matches SomeProjectSpecificEnvName:
    //public static bool IsSomeProjectSpecificEnvName(this IHostEnvironment hostEnvironment)
    //{
    //    if (hostEnvironment == null)
    //    {
    //        throw new ArgumentNullException(nameof(hostEnvironment));
    //    }

    //    return hostEnvironment.IsEnvironment(BbwtAppEnvironments.SomeProjectSpecificEnvName);
    //}
}