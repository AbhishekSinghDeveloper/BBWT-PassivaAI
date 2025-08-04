using BBWM.Core.Security;
using BBWM.Core.Web;
using BBWM.Messages;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System.Text.Json;
using System.Text.RegularExpressions;

using BBWMControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWT.Server.Api;

[Route("api/csp")]
public class ContentSecurityPolicyController : BBWMControllerBase
{
    private static readonly JsonSerializerOptions jsonSerializerOptions =
        new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = new CSPPropertyNamingPolicy()
        };

    [HttpPost("violation-report")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ReportCSPViolationAsync(
        [FromServices] IOptions<ContentSecurityPolicyOptions> options,
        [FromServices] IEmailSender emailSender,
        [FromServices] IOptionsSnapshot<EmailSettings> emailSettings)
    {
        var cspOptions = options?.Value;
        if (cspOptions?.SendViolationReport == true &&
            !string.IsNullOrEmpty(cspOptions?.ViolationSupportEmail))
        {
            var (report, formattedReport) = await GetReportAsync();

            await emailSender.SendEmail(
                "Content Security Policy violation report - " +
                    $"{Path.GetFileName(report?.SourceFile)} ({report?.LineNumber}:{report?.ColumnNumber})",
                formattedReport,
                from: emailSettings.Value.FromAddress,
                to: new[] { cspOptions.ViolationSupportEmail });
        }

        return Ok();
    }

    private async Task<(CSPViolationReportDTO, string)> GetReportAsync()
    {
        using var reader = new StreamReader(Request.Body);
        var reportFormatted = await reader.ReadToEndAsync();
        var report = default(CSPViolationReportDTO);

        try
        {
            var reportBody = JsonSerializer.Deserialize<CSPViolationReportBodyDTO>(
                reportFormatted, jsonSerializerOptions);
            report = reportBody.CspReport;

            reportFormatted = JsonSerializer.Serialize(report, jsonSerializerOptions);
            reportFormatted = Regex.Replace(
                reportFormatted.Trim().Replace(" ", "&nbsp;"), @"\r?\n", "<br/>");
        }
        catch
        {
            // Intentionally left blank. If an error occur simple
            // give the request body as string
        }

        return (report, reportFormatted);
    }

    private class CSPPropertyNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
            => (name[0] + Regex.Replace(name.Substring(1), "([A-Z])", @"-$1")).ToLowerInvariant();
    }
}
