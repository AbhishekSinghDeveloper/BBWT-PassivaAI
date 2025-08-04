namespace BBF.Reporting.Core.Interfaces;

public interface ILoggedUserService
{
    string? GetLoggedUserId();
    bool IsSystemAdmin();
}