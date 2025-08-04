namespace BBWM.Core.Exceptions;

/// <summary>
/// The exception that is thrown when the application configuration contains errors.
/// </summary>
public class ConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:BBWM.Core.Exceptions.ConfigurationException"></see> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConfigurationException(string message) : base(message) { }
}
