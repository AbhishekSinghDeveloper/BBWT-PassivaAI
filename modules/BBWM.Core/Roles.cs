namespace BBWM.Core;

/// <summary>
/// Core roles of the application.
/// </summary>
/// <remarks>
/// The development team will want a user that has both <b>SuperAdmin</b> and <b>SystemAdmin</b> access for the
/// development and internal test environments. Due to data protection laws, the development team
/// should not be given any user on a live environment (or a UAT environment containing live data)
/// except where that’s been given exceptionally careful consideration.
/// </remarks>
public static class Roles
{
    /// <summary>
    /// SuperAdmin is the role for using the BBWT3 pseudo-developer-tools – everything that allows
    /// work on the project that ought to go into GitLab. The role is used for work coming from the
    /// website rather than a typical developer submitting code after working on it in their IDE.
    /// </summary>
    /// <remarks>
    /// Has wide ranging privileges e.g. ability to design menu, manage roles and permissions, manage
    /// DB documenting tool, create reports in the reporting  tool.
    /// </remarks>
    public const string SuperAdminRole = "SuperAdmin";

    /// <summary>
    /// SystemAdmin in the live environment is a role likely to be passed over to a technical customer
    /// during the project completion process. The support team also will likely want a user that has
    /// SystemAdmin access on the live environment. With less technical customers, Blueberry support
    /// may end up permanently as the only System Admin in the live environment. Configuration features
    /// that we allow a technical customer to use should be under the SystemAdmin role.
    /// </summary>
    /// <remarks>
    /// Has the ability to manage users, organizations, email templates but not have access to security features.
    /// </remarks>
    public const string SystemAdminRole = "SystemAdmin";

    /// <summary>
    /// SystemTester is a special role originally created for users who test the application and need to
    /// login with real user details recorded by the application.
    /// </summary>
    /// <remarks>
    /// For example, a tester during development/UAT period may login with many accounts but we need to identify
    /// the tester-user as a real person in order to assign test results to him.
    /// E.g. it's used by the Feedbacks tool.
    /// </remarks>
    public const string SystemTester = "SystemTester";
}
