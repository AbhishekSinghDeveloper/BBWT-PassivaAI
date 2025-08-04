namespace BBWM.Core.Web.CookieAuth;

public class CookieAuthSettings
{
    public const string SectionName = "CookieAuthSettings";

    public string CookieName { get; set; }

    public string LoginPath { get; set; }

    public string ApiPath { get; set; }

    public int ExpireTime { get; set; }

    /// <summary>
    /// Gets or sets the amount of seconds after which Security Stamps are re-validated.
    /// Defaults to 30 minutes.
    /// </summary>
    /// <value>
    /// The amount of seconds after which Security Stamps are re-validated.
    /// </value>
    public int SecurityStampValidationInterval { get; set; } = (int)TimeSpan.FromMinutes(20).TotalSeconds;

    /// <summary>
    /// Gets or sets the amount of seconds after which the Authentication Security Stamps
    /// are re-validated. Defaults is 30 minutes.
    /// </summary>
    /// <remarks>
    /// Even though we instruct the client to delete a cookie at logout, if the cookie has been captured, it
    /// could continue to be used. This is called a "cookie replay attack". We don’t want to be calling the
    /// database tables on user access every time we make any query, but we also don’t want to let the captured
    /// cookie be used for ever. <see cref="AuthSecurityStampValidationInterval"/> determines how long we wait
    /// before verifying again that the user has logged out. <see cref="AuthSecurityStampValidationInterval"/>
    /// determines how long a user can be inactive before we ask them to re-authenticate (typically by login).
    /// Default settings below are overridden by variables at the GitLab page.
    /// </remarks>
    /// <value>
    /// The amount of seconds after which the Authentication Security Stamps are re-validated.
    /// </value>
    public int AuthSecurityStampValidationInterval { get; set; } = (int)TimeSpan.FromSeconds(90).TotalSeconds;
}
