namespace BBWM.Core.Exceptions;

/// <summary>
/// The exception that is thrown when the operation is forbidden for current server state.
/// </summary>
public sealed class ForbiddenException : ApiException
{
    /// <summary>
    /// Initialize a new instance of the <see cref="T:BBWM.Core.Exceptions.ForbiddenException"></see> class.
    /// </summary>
    public ForbiddenException() : base("The operation forbidden.") { }

    /// <summary>
    /// Initialize a new instance of the <see cref="T:BBWM.Core.Exceptions.ForbiddenException"></see> class with a specified message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ForbiddenException(string message) : base(message) { }
}
