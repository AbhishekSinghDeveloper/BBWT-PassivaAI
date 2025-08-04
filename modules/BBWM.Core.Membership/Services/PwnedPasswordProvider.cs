using BBWM.Core.Membership.Interfaces;

namespace BBWM.Core.Membership.Services;

public class PwnedPasswordProvider : IPwnedPasswordProvider
{
    public async Task<string> GetPasswordPwned(string passwordSHA1)
    {
        var urlPassword = "https://api.pwnedpasswords.com/range/";
        var client = new HttpClient { BaseAddress = new Uri(urlPassword) };
        var SHA1 = passwordSHA1.Substring(0, 5);

        var response = await client.GetAsync(SHA1);

        if (!response.IsSuccessStatusCode) return string.Empty;

        return await response.Content.ReadAsStringAsync();
    }
}
