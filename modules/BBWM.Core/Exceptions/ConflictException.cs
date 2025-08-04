namespace BBWM.Core.Exceptions;

/// <summary>
/// The exception that is thrown when a request conflicts with the current state of the system.
/// </summary>
public sealed class ConflictException : BusinessException
{
    /// <summary>
    /// Initialize a new instance of the <see cref="T:BBWM.Core.Exceptions.ConflictException"></see> class.
    /// </summary>
    public ConflictException() : base("The request conflicting with the system.") { }

    /// <summary>
    /// Initialize a new instance of the <see cref="T:BBWM.Core.Exceptions.ConflictException"></see> class with a specified message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConflictException(string message) : base(message) { }
    public ConflictException(string message, Exception innerException) : base(message, innerException) { }
}
