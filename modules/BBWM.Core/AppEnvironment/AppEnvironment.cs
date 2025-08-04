namespace BBWM.Core.AppEnvironment;

/// <summary>
/// BBWT3 overrides the environment name's detecting with own method in order to access the env. name
/// on program start and before app object is build with WebApplication.CreateBuilder()."/>
/// </summary>
public static class AppEnvironment
{
    public static string Environment
        => System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? AppEnvironmentNames.Development;

    public static bool IsEnvironment(string environmentName)
        => string.Equals(Environment, environmentName, StringComparison.OrdinalIgnoreCase);

    public static bool IsDevelopment => IsEnvironment(AppEnvironmentNames.Development);
    public static bool IsTest => IsEnvironment(AppEnvironmentNames.Test);
    public static bool IsUAT => IsEnvironment(AppEnvironmentNames.UAT);
    public static bool IsProduction => IsEnvironment(AppEnvironmentNames.Production);

    public static readonly string[] DefaultLiveTypeEnvironments = { AppEnvironmentNames.Production, AppEnvironmentNames.UAT };
}
