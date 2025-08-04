namespace BBWM.Core.Exceptions;

/// <summary>
/// The exception that is thrown when a logic of the action is not implemented.
/// </summary>
public sealed class ActionNotImplementedException : ApiException
{
    /// <summary>
    /// Initialize a new instance of the <see cref="T:BBWM.Core.Exceptions.ActionNotImplementedException"></see> class.
    /// </summary>
    public ActionNotImplementedException() : base("The action is not implemented.") { }
}
