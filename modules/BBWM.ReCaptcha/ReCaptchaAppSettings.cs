namespace BBWM.ReCaptcha;

/// <summary>
/// Settings of Google ReCaptcha (keys, API)
/// </summary>
public class ReCaptchaAppSettings
{
    /// <summary>
    /// Site key
    /// </summary>
    public string SiteKey { get; set; }

    /// <summary>
    /// Secret key
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// API link
    /// </summary>
    public string ApiLink { get; set; }

    /// <summary>
    /// Acceptable score is responsible for the minimum reCAPTCHA score at which the user will not be considered a bot.
    /// The score ranges from 0 - bot, to 1 - human.
    /// </summary>
    public decimal AcceptableScore { get; set; }
}
