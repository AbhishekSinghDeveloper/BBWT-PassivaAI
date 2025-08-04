namespace BBWM.Core.Exceptions;

/// <summary>
/// Represents an exceptions thrown when critical error happens on project data initialization
/// </summary>
public class DataInitCriticalException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:BBWM.Core.Exceptions.DataException"></see> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DataInitCriticalException(string message) : base(message) { }
    public DataInitCriticalException(string message, Exception innerException) : base(message, innerException) { }
}
