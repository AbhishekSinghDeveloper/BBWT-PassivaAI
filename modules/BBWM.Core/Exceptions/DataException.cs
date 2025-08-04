namespace BBWM.Core.Exceptions;

/// <summary>
/// Represents the base class of exceptions for layer of models and data. Use this inside models classes and context.
/// </summary>
public class DataException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:BBWM.Core.Exceptions.DataException"></see> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DataException(string message) : base(message) { }
}
