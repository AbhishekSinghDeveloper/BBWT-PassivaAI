using BBWM.Core.Web.Extensions;
using BBWM.Messages;
using BBWM.SystemData;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BBWM.ReportProblem;

public class ReportProblemService : IReportProblemService, Core.Services.IErrorNotifyService
{
    private readonly IEmailSender _emailSender;
    private readonly IOptionsSnapshot<SupportSettings> _supportSettings;
    private readonly IOptionsSnapshot<EmailSettings> _emailSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISystemDataService _systemDataService;

    public ReportProblemService(
        IEmailSender emailSender,
        IOptionsSnapshot<SupportSettings> supportSettings,
        IOptionsSnapshot<EmailSettings> emailSettings,
        IHttpContextAccessor httpContextAccessor,
        ISystemDataService systemData)
    {
        _emailSender = emailSender;
        _supportSettings = supportSettings;
        _emailSettings = emailSettings;
        _httpContextAccessor = httpContextAccessor;
        _systemDataService = systemData;
    }

    public async Task Send(ReportProblemDTO reportProblem, string userAgent, string baseUrl)
    {
        if (reportProblem is null)
            throw new ArgumentNullException("Report problem is empty. Please contact support.");

        var message = $"<div>User: {reportProblem.User} <br/>" +
                     $"Email: {reportProblem.Email} <br/>" +
                     $"From : {baseUrl} " +

                     $"Time: {reportProblem.Time} <br/>" +
                    $"Build Version of the site: - {_systemDataService.GetVersionInfo()?.FullProductVersion} <br/>" +
                    $"Browser string: {userAgent} <br/><br/> " +

                     $"Description: {reportProblem.Description} <br/><br/>" +
                      $"Severity: {reportProblem.Severity}" +
                    $"</div>";

        var supportSettings = GetValidatedSupportSettings();
        var emailSettings = GetValidatedEmailSettings();

        await _emailSender.SendEmail(reportProblem.Subject, message, emailSettings.FromAddress, null, null,
            supportSettings.EmailAddress1, supportSettings.EmailAddress2, supportSettings.EmailAddress3, supportSettings.EmailAddress4);
    }

    public async Task AutoSend(Exception exception)
    {
        if (exception is null) return;

        var errorLogDTO = new ErrorLogDTO
        {
            ExceptionType = "Server side exception",
            ExceptionMessage = exception.Message,
            StackTrace = exception.ToString(),
            PathBase = _httpContextAccessor.HttpContext.GetDomainUrl(),
            Path = _httpContextAccessor.HttpContext.Request.Path
        };

        await AutoSend(errorLogDTO);
    }

    public async Task AutoSend(ErrorLogDTO errorLogDTO)
    {
        if (errorLogDTO is null) return;

        var supportSettings = GetValidatedSupportSettings();
        var emailSettings = GetValidatedEmailSettings();

        // Generates a body of the report.
        // (!) Note well: Current PTS System (Aug 18 2020) uses a regex pattern to fetch values from pairs
        // {title: value} (like {Path: errorLogDTO.Path} below).
        // The values help PTS to recognize how to process the email's body on the PTS side.
        // Ideally we would need some code interface to explicitely define relationship between BBWT3-based project
        // and PTS, but at the moment we just hardcode the body as it's done here.
        // Therefore, if you change any title name or remove it, then PTS may process it incorrectly
        // (may skip creating PTS ticket). Do keep it in mind.
        // If you do make a change in the body format, it's recommended to give a note to PTS managers.

        var message = $"<div>Exception type: {errorLogDTO.ExceptionType} <br/><br/>" +
                      $"Description: {errorLogDTO.ExceptionMessage} <br/><br/>" +
                      $"Path base: {errorLogDTO.PathBase} <br/>" +
                      $"Path: {errorLogDTO.Path} <br/><br/>" +
                      $"Stack trace: {errorLogDTO.StackTrace}" +
                      $"</div>";

        await _emailSender.SendEmail($"ERROR {errorLogDTO.ExceptionType}", message, emailSettings.FromAddress, null, null,
            supportSettings.EmailAddress1, supportSettings.EmailAddress2, supportSettings.EmailAddress3, supportSettings.EmailAddress4);
    }

    /// <summary>
    /// ReportProblem service in current version is supposed to implement notification sending that
    /// is handled by the Core module on core exceptions occurs. The Core module only uses an interface
    /// to trigger notification. The startup project sets dependency to delegate notification implementation
    /// to this ReportProblem Service.
    /// </summary>
    public Task NotifyOnException(Exception exception)
        => AutoSend(exception);

    private SupportSettings GetValidatedSupportSettings()
    {
        var settings = _supportSettings.Value;
        if (settings is null)
            throw new ArgumentNullException("Support settings value is empty");

        if (settings.EmailAddress1 is null && settings.EmailAddress2 is null &&
             settings.EmailAddress3 is null && settings.EmailAddress4 is null)
            throw new ArgumentNullException("Support settings emails not defined");

        return settings;
    }

    private EmailSettings GetValidatedEmailSettings()
    {
        var settings = _emailSettings.Value;
        if (settings is null)
            throw new ArgumentNullException("Email settings value is empty");

        if (string.IsNullOrEmpty(settings.FromAddress))
            throw new ArgumentNullException("From address of email settings not defined");

        return settings;
    }
}
