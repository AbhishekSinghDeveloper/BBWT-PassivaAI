namespace BBWM.Core.AppEnvironment;

public interface IAppEnvironmentService
{
    /// <summary>
    /// Determines whether current application environment matches one of live type environments (e.g. Production, UAT).
    /// The matching criteria can be determined by a setting (e.g. in appsettings.json) that contains a list of live environments.
    /// This method helps to manage this centrally.
    /// </summary>
    bool IsLiveTypeEnvironment();
}
