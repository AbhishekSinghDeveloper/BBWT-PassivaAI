namespace BBWM.Core.Exceptions;

/// <summary>
/// Represents the base class for API layer. Use this inside controllers.
/// </summary>
public class ApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:BBWM.Core.Exceptions.ApiException"></see> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ApiException(string message) : base(message) { }
}
