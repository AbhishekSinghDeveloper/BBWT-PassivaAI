namespace BBWM.Core.Exceptions;

/// <summary>
/// The exception that is thrown when the object being operated on is null.
/// </summary>
public class ObjectNotExistsException : BusinessException
{
    /// <summary>
    /// Initialize a new instance of the <see cref="T:BBWM.Core.Exceptions.ObjectNotExistsException"></see> class.
    /// </summary>
    public ObjectNotExistsException() : base("Object doesn't exist.") { }

    /// <summary>
    /// Initialize a new instance of the <see cref="T:BBWM.Core.Exceptions.ObjectNotExistsException"></see> class with a specified message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ObjectNotExistsException(string message) : base(message) { }
}
