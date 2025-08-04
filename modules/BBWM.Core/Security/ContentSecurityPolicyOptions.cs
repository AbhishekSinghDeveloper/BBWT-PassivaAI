namespace BBWM.Core.Security;

public class ContentSecurityPolicyOptions
{
    public const string SectionName = "ContentSecurityPolicy";

    /// <summary>
    /// Gets or sets whether Content Security Policy is enabled. Default is <c>true</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// IMPORTANT! Please note that enabling Content Security Policy at the same time that PWA can make the
    /// latter to stop working. See <see href="https://pts.bbconsult.co.uk/issueEditor?id=258175">this issue</see> for details.
    /// </para>
    /// <para>
    /// Content Security Policy uses nonce (number used once) to determine whether or not scripts are
    /// trustworthy to run and this can conflict with the hashes PWA compute to the files during build time.
    /// </para>
    /// </remarks>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether a to create a report-only Content Security Policy which violation reports will
    /// be sent to endpoint <see cref="ViolationReportUri"/>. Default is <c>false</c>.
    /// </summary>
    public bool SendViolationReport { get; set; } = false;

    /// <summary>
    /// Gets or sets the email address where all Content Security Policy violation reports will be forwarded to.
    /// </summary>
    public string ViolationSupportEmail { get; set; }

    /// <summary>
    /// Gets or sets the endpoint where all Content Security Policy violation reports will be sent to before
    /// forwarding them to <see cref="ViolationSupportEmail"/>. Default is <c>/api/csp/violation-report</c>.
    /// </summary>
    public string ViolationReportUri { get; set; } = "/api/csp/violation-report";
}
