using Microsoft.Extensions.Hosting;

namespace BBWM.Core.AppEnvironment;

/// <summary>
/// Application environments, specific for BBWT-projects deployment workflow
/// 
/// Note! A customer project shouldn't extend environments list in BBWM.Core, instead
/// you can add it into the template (BBWT.*) project part. See BBWT.Server.Extensions.BbwtAppEnvironments.
public static class AppEnvironmentNames
{
    public static readonly string Development = Environments.Development;
    public static readonly string Test = "Test";
    public static readonly string UAT = "UAT";
    public static readonly string Production = Environments.Production;
}
