using BBWM.Core.Exceptions;

namespace BBWM.Core.Membership.Exceptions;

/// <summary>
/// The exception that is thrown when a login has failed.
/// </summary>
public class LoginFailedException : BusinessException
{
    /// <summary>
    /// Initialize a new instance of the <see cref="T:BBWM.Core.Exceptions.LoginFailedException"></see> class with data for audit.
    /// </summary>
    /// <param name="userMessage">The message that describes the error for the end-user.</param>
    /// <param name="auditMessage">The message used for the audit and described the real reason of the error.</param>
    public LoginFailedException(string userMessage, string auditMessage = null) : base(userMessage) =>
        AuditMessage = auditMessage ?? userMessage;

    public string AuditMessage { get; set; }
}
