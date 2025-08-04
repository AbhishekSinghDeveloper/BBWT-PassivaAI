namespace BBWM.Core.Membership.Exceptions;

/// <summary>
/// The exception that is thrown when a login has failed due to wrong credentials.
/// </summary>
public class WrongCredentialsException : LoginFailedException
{
    /// <summary>
    /// Initialize a new instance of the <see cref="T:BBWM.Core.Exceptions.WrongCredentialsException"></see> class with data for audit.
    /// </summary>
    /// <param name="userMessage">The message that describes the error.</param>
    public WrongCredentialsException(string userMessage, string auditMessage = null) : base(userMessage, auditMessage) { }
}
