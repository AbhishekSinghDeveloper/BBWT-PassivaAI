using BBWM.Core.Security;

namespace BBWT.Server.Extensions;

internal static class ContentSecurityPolicyExtensions
{
    public static HeaderPolicyCollection AddContentSecurityPolicy(
        this HeaderPolicyCollection headers, ContentSecurityPolicyOptions cspOptions)
        => cspOptions.Enabled
            ? headers.CreatePolicy().CreateReportOnlyPolicy(cspOptions)
            : headers;

    private static HeaderPolicyCollection CreatePolicy(this HeaderPolicyCollection headers)
        => headers.AddContentSecurityPolicy(
            csp =>
            {
                csp.AddBaseUri().Self();
                csp
                    .AddScriptSrc()
                    // TODO: temporarily commented to check out how it affects the blank page issue (new browser tab is opened blank)
                    // .StrictDynamic()
                    .WithNonce()
                    // .UnsafeInline()
                    // This is required to load formio components
                    // We must find a way to only enable this if formio is going to be used.
                    .UnsafeEval()
                    .OverHttps();
                csp.AddObjectSrc().None();
            });

    private static HeaderPolicyCollection CreateReportOnlyPolicy(
        this HeaderPolicyCollection headers, ContentSecurityPolicyOptions cspOptions)
        => cspOptions.SendViolationReport && !string.IsNullOrEmpty(cspOptions.ViolationReportUri)
            ? headers.AddContentSecurityPolicyReportOnly(
                csp =>
                {
                    csp.AddCustomDirective("require-trusted-types-for", "'script'");
                    csp.AddCustomDirective("trusted-types", "angular");
                    csp.AddStyleSrc().StrictDynamic().UnsafeInline();
                    csp.AddReportUri().To(cspOptions.ViolationReportUri);
                })
            : headers;
}
