using BBWM.Core.Utils;
using BBWM.SystemSettings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Text.Json;

namespace BBWM.ReCaptcha;

public class ReCaptchaService : IReCaptchaService
{
    private readonly ReCaptchaAppSettings _reCaptchaAppSettings;
    private readonly ReCaptchaSettings _reCaptchaSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReCaptchaService> _logger;

    public ReCaptchaService(
        IOptionsSnapshot<ReCaptchaAppSettings> reCaptchaSettings,
        ISettingsService settingsService,
        IHttpClientFactory httpClientFactory,
        ILogger<ReCaptchaService> logger)
    {
        _reCaptchaSettings = settingsService.GetSettingsSection<ReCaptchaSettings>();
        _reCaptchaAppSettings = reCaptchaSettings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>Verifying user's response with reCAPTCHA using Google API to ensure the token is valid</summary>
    /// <param name="reCaptchaToken">ReCaptcha token</param>
    /// <returns>bool</returns>
    public async Task<bool> CheckReCaptchaAsync(string reCaptchaToken, CancellationToken cancellationToken = default)
    {
        if (_reCaptchaSettings.ValidateOnLoginEnabled is null || !(bool)_reCaptchaSettings.ValidateOnLoginEnabled) return true;

        if (reCaptchaToken is null) return false;

        if (string.IsNullOrWhiteSpace(_reCaptchaAppSettings.SecretKey) ||
            string.IsNullOrWhiteSpace(_reCaptchaAppSettings.ApiLink))
        {
            _logger.LogError("ReCaptcha settings are not configured in application environment configuration");
            return true;
        }

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{_reCaptchaAppSettings.ApiLink}?secret={_reCaptchaAppSettings.SecretKey}&response={reCaptchaToken}");
        using var client = _httpClientFactory.CreateClient("reCaptcha");
        var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode) throw new HttpRequestException("Request to response API failed");

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var captchaResponse = JsonSerializer.Deserialize<CaptchaResponse>(jsonResponse, JsonSerializerOptionsProvider.Options);
        var result = captchaResponse is not null && captchaResponse.Score >= _reCaptchaAppSettings.AcceptableScore;

        return result;
    }
}