namespace BBWM.Core;

public static class AggregatedRoles
{
    /// <summary>
    /// Any authenticated user allowed
    /// </summary>
    public const string Authenticated = "Authenticated";

    /// <summary>
    /// Anyone allowed, either authenticated or not.
    /// </summary>
    public const string Anyone = "Anyone";

    /// <summary>
    /// Noone user allowed
    /// </summary>
    public const string Noone = "Noone";
}
