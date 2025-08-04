using Microsoft.AspNetCore.Identity;

namespace BBWM.Core.Membership;

public class AuthSecurityStampValidatorOptions : SecurityStampValidatorOptions
{
    /// <summary>
    /// Gets or sets the <see cref="TimeSpan"/> after which the Authentication Security Stamps
    /// are re-validated. Defaults to 30 minutes.
    /// </summary>
    /// <value>
    /// The <see cref="TimeSpan"/> after which the Authentication Security Stamps are re-validated.
    /// </value>
    public TimeSpan AuthValidationInterval { get; set; }
}
