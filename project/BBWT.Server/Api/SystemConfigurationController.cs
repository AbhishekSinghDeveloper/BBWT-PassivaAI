using BBWM.Core;
using BBWM.Core.Utils;
using BBWM.Core.Web.CookieAuth;
using BBWM.Core.Web.Filters;
using BBWM.FileStorage;
using BBWM.LoadingTime;
using BBWM.Maintenance;
using BBWM.Messages;
using BBWM.SystemSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ClaimTypes = System.Security.Claims.ClaimTypes;
using ControllerBase = BBWM.Core.Web.ControllerBase;
using CoreRoles = BBWM.Core.Roles;

namespace BBWT.Server.Api;

[Produces("application/json")]
[Route("api/system-configuration")]
[ReadWriteAuthorize(ReadRoles = AggregatedRoles.Anyone,
    WriteRoles = CoreRoles.SuperAdminRole + "," + CoreRoles.SystemAdminRole)]
public class SystemConfigurationController : ControllerBase
{
    private const string UploadLogoImageOperationName = "ProjectLogoImageUploading";
    private const string UploadLogoIconOperationName = "ProjectLogoIconUploading";

    private readonly ISettingsService _settingsService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IHubContext<MaintenanceHub> _maintenanceHubContext;
    private readonly bool? _pwaEnabled;

    public SystemConfigurationController(
        ISettingsService settingsService,
        IFileStorageService fileStorageService,
        IHubContext<MaintenanceHub> maintenanceHubContext,
        IConfiguration configuration)
    {
        _settingsService = settingsService;
        _fileStorageService = fileStorageService;
        _maintenanceHubContext = maintenanceHubContext;
        _pwaEnabled = configuration.GetValue<bool?>("PwaEnabled");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Load() =>
        Ok(await _settingsService.Load(
                User.Identity.IsAuthenticated
                    ? null
                    : new[]
                    {
                            SettingsName.UserPasswordSettings,
                            SettingsName.RegistrationSettings,
                            SettingsName.ProjectSettings,
                            SettingsName.PwaSettings,
                            SettingsName.FacebookSsoSettings,
                            SettingsName.GoogleSsoSettings,
                            SettingsName.LinkedInSsoSettings
                    }
            )
        );

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SettingsDTO[] config)
    {
        var newProjectSettings = config.SingleOrDefault(section => section.SectionName == "ProjectSettings");
        if (newProjectSettings is not null)
        {
            // Removing old logo images
            await DeleteProjectSettingsLogoFiles(newProjectSettings.Value);
        }

        var result = await _settingsService.Save(config);

        if (newProjectSettings is not null)
        {
            // Unbinding files from user and operation name to complete operation
            await CompleteLogoImagesUploading();
        }

        var newMaintenanceSettingsSection = result.SingleOrDefault(section => section.SectionName == "MaintenanceSettings");
        if (newMaintenanceSettingsSection is not null)
        {
            var serializedSettings = JsonSerializer.Serialize(newMaintenanceSettingsSection.Value, JsonSerializerOptionsProvider.Options);
            var newMaintenanceSettings = JsonSerializer.Deserialize<MaintenanceSettings>(serializedSettings, JsonSerializerOptionsProvider.Options);
            await _maintenanceHubContext.Clients.All.SendAsync("InfoUpdated", newMaintenanceSettings);
        }

        return Ok(result);
    }

    [HttpGet("maintenance-settings")]
    [DoNotResetAuthCookie]
    public IActionResult MaintenanceSettings() =>
        Ok(_settingsService.GetSettingsSection<MaintenanceSettings>());

    [HttpPost("loading-times-settings")]
    public IActionResult SetLoadingTimeSettings(bool recordLoadingTime, CancellationToken cancellationToken)
    {
        var loadingTimeSettings = _settingsService.GetSettingsSection<LoadingTimeSettings>();
        loadingTimeSettings.RecordLoadingTime = recordLoadingTime;
        _settingsService.SaveSettingsSection(loadingTimeSettings);
        return Ok();
    }

    [HttpGet("project-settings-images")]
    public async Task<IActionResult> GetProjectSettingsImages()
    {
        var settings = _settingsService.GetSettingsSection<ProjectSettings>();

        var logoIconFile = settings.LogoIconId == null ?
            new FileDetailsDTO() { Url = ProjectSettings.DefaultLogoIconUrl }
            : await _fileStorageService.Get(settings.LogoIconId.Value);

        var logoImageFile = settings.LogoImageId == null ?
            new FileDetailsDTO() { Url = ProjectSettings.DefaultLogoImageUrl }
            : await _fileStorageService.Get(settings.LogoImageId.Value);

        return Ok(
            new ProjectSettingsImages
            {
                LogoIcon = logoIconFile,
                LogoImage = logoImageFile
            });
    }

    [HttpPost("upload-logo-image")]
    [AllowedFormFileFormats(1000000, "image/png", "image/jpeg", "image/gif")]
    public async Task<IActionResult> UploadProjectLogoImage(IFormCollection formData, CancellationToken cancellationToken)
    {
        var files = Request.Form.Files;

        if (files is null) return BadRequest("There is no uploaded file.");
        if (files.Count != 1) return BadRequest("The count of uploaded files should be 1.");

        // Binding files to user and operation name for removing old files
        var additionalData =
            formData.Keys.ToDictionary<string, string, string>(key => key, key => formData[key]);
        additionalData.Add("max_size", "10000");
        additionalData.Add("thumbnail_size", "400");
        additionalData.Add("user_id", User.FindFirstValue(ClaimTypes.NameIdentifier));
        additionalData.Add("operation_name", UploadLogoImageOperationName);

        return Ok((await _fileStorageService.UploadFiles(files.ToArray(), additionalData, cancellationToken)).SuccessfullyUploadedFiles[0]);
    }

    [HttpPost("upload-logo-icon")]
    [AllowedFormFileFormats(100000, "image/x-icon")]
    public async Task<IActionResult> UploadProjectLogoIcon(IFormCollection formData, CancellationToken cancellationToken)
    {
        var files = Request.Form.Files;

        if (files is null) return BadRequest("There is no uploaded file.");
        if (files.Count != 1) return BadRequest("The count of uploaded files should be 1.");

        // Binding files to user and operation name for removing old files
        var additionalData =
            formData.Keys.ToDictionary<string, string, string>(key => key, key => formData[key]);
        additionalData.Add("max_size", "50");
        additionalData.Add("thumbnail_size", "50");
        additionalData.Add("user_id", User.FindFirstValue(ClaimTypes.NameIdentifier));
        additionalData.Add("operation_name", UploadLogoIconOperationName);

        return Ok((await _fileStorageService.UploadFiles(files.ToArray(), additionalData, cancellationToken)).SuccessfullyUploadedFiles[0]);
    }

    [HttpGet("email-settings")]
    public IActionResult EmailsSettings([FromServices] IOptionsSnapshot<EmailSettings> emailSettings)
    {
        var settings = emailSettings.Value;
        if (settings is not null)
        {
            return Ok(
                new
                {
                    smtp = settings.SMTP,
                    settings.Port,
                    settings.UserName,
                    settings.EnableSsl,
                    settings.AdminAddress,
                    settings.FromAddress,
                    settings.UseDefaultCredentials,
                    settings.UseTestAddressForOutgoingEmails,
                    settings.TestEmailAddress
                });
        }
        return NoContent();
    }

    [HttpGet("test-email")]
    public async Task<IActionResult> SendTestEmail(
        [FromServices] IEmailSender emailSender,
        [FromServices] IOptionsSnapshot<EmailSettings> emailSettings)
    {
        var additionalInfo = new StringBuilder("Test Email<br/><br/>");
        additionalInfo.AppendFormat("OS Name: {0}<br/>", Environment.OSVersion.Platform);
        additionalInfo.AppendFormat("Server IP: {0}<br/>", HttpContext.Connection.LocalIpAddress);

        await emailSender.SendEmail(
            "Email Sender testing",
            additionalInfo.ToString(),
            from: emailSettings.Value.FromAddress,
            to: emailSettings.Value.AdminAddress);

        return NoContent();
    }

    [HttpGet("pwa-enabled")]
    [AllowAnonymous]
    public IActionResult GetPwaEnabled() => Ok(_pwaEnabled ?? false);

    private async Task DeleteProjectSettingsLogoFiles(object newProjectSettingsJson, CancellationToken cancellationToken = default)
    {
        var oldProjectSettings = _settingsService.GetSettingsSection<ProjectSettings>();
        var serializedSettings = JsonSerializer.Serialize(newProjectSettingsJson, JsonSerializerOptionsProvider.Options);
        var newProjectSettings = JsonSerializer.Deserialize<ProjectSettings>(serializedSettings, JsonSerializerOptionsProvider.Options);

        if (oldProjectSettings.LogoImageId is not null &&
            newProjectSettings.LogoImageId != oldProjectSettings.LogoImageId)
            await _fileStorageService.DeleteFile((int)oldProjectSettings.LogoImageId, cancellationToken);

        if (oldProjectSettings.LogoIconId is not null &&
            newProjectSettings.LogoIconId != oldProjectSettings.LogoIconId)
            await _fileStorageService.DeleteFile((int)oldProjectSettings.LogoIconId, cancellationToken);
    }

    private async Task CompleteLogoImagesUploading(CancellationToken cancellationToken = default)
    {
        await _fileStorageService.CompleteUsersFilesUploadingOperation(
            User.FindFirstValue(ClaimTypes.NameIdentifier), UploadLogoImageOperationName, cancellationToken);
        await _fileStorageService.CompleteUsersFilesUploadingOperation(
            User.FindFirstValue(ClaimTypes.NameIdentifier), UploadLogoIconOperationName, cancellationToken);
    }
}