namespace BBWM.Core.Exceptions;

/// <summary>
/// The exception that is thrown when the model is invalid.
/// </summary>
public sealed class InvalidModelException : ApiException
{
    /// <summary>
    /// Initialize a new instance of the <see cref="T:BBWM.Core.Exceptions.InvalidModelException"></see> class.
    /// </summary>
    public InvalidModelException() : base("Model is invalid.") { }

    /// <summary>
    /// Initialize a new instance of the <see cref="T:BBWM.Core.Exceptions.InvalidModelException"></see> class with a specified message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidModelException(string message) : base(message) { }
}
