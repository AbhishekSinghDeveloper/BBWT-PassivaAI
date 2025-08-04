using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Exceptions;
using BBWM.Core.Membership.Filters;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web.Extensions;
using BBWM.JWT;
using BBWM.ReCaptcha;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Claims;
using ClaimTypes = System.Security.Claims.ClaimTypes;
using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWT.Server.Api;

[Produces("application/json")]
[Route("api/account"), ResponseCache(NoStore = true)]
public class AccountApiController : ControllerBase
{
    private readonly ILogger<AccountApiController> _logger;
    private readonly IUserService _userService;
    private readonly IUser2FAService _user2FAService;
    private readonly IDataService _dataService;
    private readonly IUserDataService _userDataService;
    private readonly ISecurityService _securityService;
    private readonly IJwtService _jwtService;

    private readonly IOptionsSnapshot<ReCaptchaAppSettings> _reCaptchaSettings;

    private const string Scheme = "https"; // hardcoded because Request.Scheme not reliable with load balanced architecture especially with Azure


    public AccountApiController(
        IDataService dataService,
        IUserService userService,
        IUser2FAService user2FAService,
        IUserDataService userDataService,
        ISecurityService securityService,
        IJwtService jwtService,
        ILogger<AccountApiController> logger,
        IOptionsSnapshot<ReCaptchaAppSettings> reCaptchaSettings)
    {
        _logger = logger;
        _dataService = dataService;
        _userService = userService;
        _user2FAService = user2FAService;
        _userDataService = userDataService;
        _securityService = securityService;
        _jwtService = jwtService;
        _reCaptchaSettings = reCaptchaSettings;
    }

    [HttpPost]
    [Route("recover-password")]
    [AllowAnonymous]
    public Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordDTO recoverPassword, CancellationToken cancellationToken = default)
        => NoContent(() => _userService.RecoverPassword(recoverPassword, cancellationToken));

    [HttpPost]
    [Route("reset-password")]
    [AllowAnonymous]
    public Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPassword, CancellationToken cancellationToken = default)
        => NoContent(() => _userService.ResetPassword(resetPassword, cancellationToken));

    [HttpPost]
    [Route("activate")]
    [AllowAnonymous]
    public Task<IActionResult> Activate([FromBody] ResetPasswordDTO dto, CancellationToken cancellationToken = default)
        => NoContent(() => _userService.Activate(dto, cancellationToken));

    [HttpPost]
    [Route("confirm-email")]
    [AllowAnonymous]
    public Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDTO dto, CancellationToken cancellationToken = default)
        => NoContent(() => _userService.ConfirmEmail(dto, cancellationToken));

    [HttpPost]
    [Route("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDTO dto, CancellationToken cancellationToken = default)
    {
        var signUpResult = await _userService.Register(dto, cancellationToken);
        _logger.LogInformation($"Registered user: {dto.Email}");
        return Ok(signUpResult);
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.Login(dto, cancellationToken);
            await _userService.CheckBrowserLoginAsync(result, dto.Email, dto.Fingerprint, cancellationToken);

            return Ok(result);
        }
        catch (Exception exception)
        {
            _logger.LogDebug($"Dialog popup: {exception.Message}.");

            await _dataService.Create<LoginAudit, LoginAuditDTO>(
                new LoginAuditDTO
                {
                    Datetime = DateTimeOffset.Now,
                    Email = dto.Email,
                    Ip = HttpContext.GetUserIp(),
                    Browser = dto.Browser,
                    Fingerprint = dto.Fingerprint,
                    Location = null,
                    Result = exception is LoginFailedException loginFailedException
                        ? loginFailedException.AuditMessage
                        : exception.Message
                }, cancellationToken);

            return HandleException(exception);
        }
    }

    [HttpGet]
    [Route("token")]
    public IActionResult GetToken() => Ok(_jwtService.GenerateToken(User.Identity.Name));

    /// <summary>
    /// Returns Site Key for Google reCAPTCHA.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("recaptcha-site-key")]
    [AllowAnonymous]
    public IActionResult GetReCaptchaSiteKey()
    {
        var reCaptchaSettings = _reCaptchaSettings.Value;
        return Ok(reCaptchaSettings is not null ? reCaptchaSettings.SiteKey : string.Empty);
    }

    [HttpPost]
    [Route("logout")]
    [IgnoreSetup2FaCheck]
    public Task<IActionResult> Logout()
        => NoContent(async () =>
        {
            await _userService.Logout();
            _logger.LogInformation("User logged out successfully.");
        });

    /// <summary>
    /// Returns the number of seconds before the blocking for a user's IP address expires.
    /// </summary>
    [HttpGet]
    [Route("ip-locking-time")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLockingTimeByIp(CancellationToken cancellationToken = default)
    {
        var result = await _securityService.GetLongestActiveLockingByIp(HttpContext.GetUserIp(), cancellationToken);

        if (result is null)
            return Ok(string.Empty);

        var timeSpan = result.LockoutEnd.Subtract(DateTime.Now);
        return Ok(timeSpan.Seconds.ToString());
    }

    /// <summary>
    /// Returns QR code and shared key to enable 2FA.
    /// </summary>
    [HttpGet]
    [Route("me/2fa-enabling-data")]
    [IgnoreSetup2FaCheck]
    public async Task<IActionResult> Get2FAEnablingDataForCurrentUser() =>
        Ok(await _user2FAService.Get2FAEnablingData(User.FindFirstValue(ClaimTypes.NameIdentifier)));

    /// <summary>
    /// Enables 2FA.
    /// </summary>
    /// <param name="dto">The verification data that consists of QR code and shared key.</param>
    [HttpPost]
    [Route("me/enable-2fa")]
    [IgnoreSetup2FaCheck]
    public async Task<IActionResult> Enable2FAForCurrentUser([FromBody] Enabling2FADTO dto, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await _user2FAService.Enable2FA(dto, userId, cancellationToken);

        _logger.LogInformation($"User with ID '{userId}' has enabled 2FA with an authenticator app.");
        return RedirectToAction(nameof(Generate2FAOrU2FRecoveryCodesForCurrentUser));
    }

    [HttpPost]
    [Route("me/disable-2fa")]
    public async Task<IActionResult> Disable2FAForCurrentUser([FromBody] Disabling2FADTO dto, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await _user2FAService.Disable2FA(userId, dto.Code, cancellationToken);

        _logger.LogInformation($"User with ID '{userId}' has disabled 2FA.");
        return NoContent();
    }

    [HttpPost]
    [Route("me/enable-u2f")]
    [IgnoreSetup2FaCheck]
    public async Task<IActionResult> EnableU2FForCurrentUser(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await _userService.EnableU2F(userId, cancellationToken);

        _logger.LogInformation($"User with ID '{userId}' has enabled U2F.");
        return RedirectToAction(nameof(Generate2FAOrU2FRecoveryCodesForCurrentUser));
    }

    [HttpPost]
    [Route("me/disable-u2f")]
    public async Task<IActionResult> DisableU2FForCurrentUser(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await _userService.DisableU2F(userId, cancellationToken);

        _logger.LogInformation($"User with ID '{userId}' has disabled U2F.");
        return NoContent();
    }

    [HttpGet]
    [Route("me/2fa-u2f-recovery-codes/{isNeedNew?}")]
    [IgnoreSetup2FaCheck]
    public async Task<IActionResult> Generate2FAOrU2FRecoveryCodesForCurrentUser(bool? isNeedNew, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var recoveryCode = await _userService.Generate2FaOrU2FRecoveryCodes(userId, isNeedNew ?? false, cancellationToken);

        _logger.LogInformation($"User with ID '{userId}' has generated new recovery codes for 2FA or U2F.");
        return Ok(recoveryCode);
    }

    /// <summary>
    /// Returns data for device registration.
    /// </summary>
    [HttpGet]
    [Route("me/generate-u2f-device-registration-challenge")]
    [IgnoreSetup2FaCheck]
    public async Task<IActionResult> GenerateU2FDeviceRegistrationChallengeForCurrentUser(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(await _userService.GenerateU2FDeviceRegistrationChallenge(userId, $"{Scheme}://{Request.Host}", cancellationToken));
    }

    [HttpPost]
    [Route("me/register-u2f-device")]
    [IgnoreSetup2FaCheck]
    public async Task<IActionResult> RegisterU2FDeviceForCurrentUser(
        [FromBody] U2FRegistrationResponseDTO u2FRegistrationResponseDto,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await _userService.RegisterU2FDevice(userId, u2FRegistrationResponseDto, cancellationToken);

        _logger.LogInformation($"User with ID '{userId}' has registered U2F device.");
        return NoContent();
    }

    [HttpPost]
    [Route("authenticate-u2f-device")]
    [AllowAnonymous]
    public async Task<IActionResult> AuthenticateU2FDevice(
        [FromBody] U2FAuthenticationResponseDTO u2FAuthenticationResponseDto,
        CancellationToken cancellationToken = default)
    {
        await _userService.AuthenticateU2FDevice(u2FAuthenticationResponseDto, cancellationToken);

        _logger.LogInformation($"User with ID '{u2FAuthenticationResponseDto.UserId}' has authenticated.");
        return NoContent();
    }

    [HttpPost]
    [Route("recovery-code-login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWithRecoveryCode([FromBody] RecoveryCodeDTO dto, CancellationToken cancellationToken = default)
    {
        var user = await _userDataService.Get(dto.UserId, cancellationToken);
        await _userService.Login(dto, cancellationToken);

        return Ok(user);
    }

    /// <summary>
    /// Checks availability of the recovery code.
    /// </summary>
    /// <param name="dto">Information about user and recovery code.</param>
    [HttpGet]
    [Route("check-recovery-code")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckRecoveryCode(RecoveryCodeDTO dto, CancellationToken cancellationToken = default)
    {
        await _userService.CheckRecoveryCodeExists(dto, cancellationToken);
        return NoContent();
    }

    [HttpGet, Route("activation-info")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAccountActivationInfo(string userId, string code, CancellationToken cancellationToken = default) =>
        Ok(await _userService.GetAccountActivationInfo(userId, code, cancellationToken));

    [HttpGet]
    [Route("check-two-factor-code")]
    [AllowAnonymous]
    public async Task<IActionResult> Check2FaCode(Checking2FADTO dto, CancellationToken cancellationToken = default)
    {
        await _user2FAService.Verify2FACode(dto, cancellationToken);
        return NoContent();
    }


    [HttpGet, Route("two-factor-info")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTwoFactorInfo(TwoFactorInfoRequestDTO dto, CancellationToken cancellationToken = default) =>
        Ok(await _userService.GetTwoFactorInfo(dto, cancellationToken));

    protected IActionResult HandleException(Exception exception)
    {
        if (exception is WrongCaptchaException || exception is LoginFailedException)
            return Unauthorized(exception.Message);

        throw exception;
    }
}
