namespace BBWM.ReCaptcha;

public interface IReCaptchaService
{
    /// <summary>Verifying user's response with reCAPTCHA using Google API to ensure the token is valid</summary>
    /// <param name="reCaptchaToken">ReCaptcha Token"</param>
    Task<bool> CheckReCaptchaAsync(string reCaptchaToken, CancellationToken cancellationToken = default);
}
