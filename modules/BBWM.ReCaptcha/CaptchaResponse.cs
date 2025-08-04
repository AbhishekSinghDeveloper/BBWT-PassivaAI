namespace BBWM.ReCaptcha;

/// <summary>
/// Company security ticket definition
/// </summary>
public class CaptchaResponse
{
    /// <summary>
    /// success
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// score
    /// </summary>
    public decimal Score { get; set; }

    /// <summary>
    /// action
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    /// challenge_ts : timestamp of the challenge load (ISO format yyyy-MM-dd'T'HH:mm:ssZZ)
    /// </summary>
    public DateTime Challenge_ts { get; set; }

    /// <summary>
    /// hostname
    /// </summary>
    public string Hostname { get; set; }
}
