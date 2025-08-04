namespace BBWM.Core.Membership.Interfaces;

public interface IPwnedPasswordProvider
{
    Task<string> GetPasswordPwned(string passwordSHA1);
}
