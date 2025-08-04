namespace BBWM.Core.Exceptions;

/// <summary>
/// Represents the base class for layer of a business-logic. Use this inside services.
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:BBWM.Core.Exceptions.BusinessException"></see> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BusinessException(string message) : base(message) { }
    public BusinessException(string message, Exception innerException) : base(message, innerException) { }
}
