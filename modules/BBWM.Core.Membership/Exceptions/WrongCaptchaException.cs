using BBWM.Core.Exceptions;

namespace BBWM.Core.Membership.Exceptions;

/// <summary>
/// The exception that is thrown when a captcha code is wrong.
/// </summary>
public class WrongCaptchaException : ApiException
{
    /// <summary>
    /// Initialize a new instance of the <see cref="T:BBWM.Core.Exceptions.WrongCaptchaException"></see> class.
    /// </summary>
    public WrongCaptchaException() : base("The captcha code is wrong.") { }

    /// <summary>
    /// Initialize a new instance of the <see cref="T:BBWM.Core.Exceptions.WrongCaptchaException"></see> class with a specified message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public WrongCaptchaException(string message) : base(message) { }
}
