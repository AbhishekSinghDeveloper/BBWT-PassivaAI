using BBWM.Core.Membership.Model;
using BBWM.Core.Test;
using BBWM.Core.Test.Utils;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;
using Xunit;

namespace BBWM.Core.Membership.Test;

public class UserClaimsResolverTests
{
    [Fact]
    public void Resolve_Empty_Claims()
    {
        // Arrange
        UserClaimsResolver userClaimsResolver = new();

        // Act
        Dictionary<string, string> claims = userClaimsResolver.Resolve(default, default, default, default);

        // Assert
        Assert.Empty(claims);
    }

    [Fact]
    public async Task Resolve_Single_Claim()
    {
        // Arrange
        var (userClaimsResolver, user) = await CreateResolverAsync(("C1", "C1V1"));

        // Act
        Dictionary<string, string> claims = userClaimsResolver.Resolve(user, default, default, default);

        // Assert
        KeyValuePair<string, string> claim = Assert.Single(claims);
        Assert.Equal("C1", claim.Key);
        Assert.Equal("C1V1", claim.Value);
    }

    [Fact]
    public async Task Resolve_Multiple_Claims()
    {
        // Arrange
        var (userClaimsResolver, user) = await CreateResolverAsync(("C1", "C1V1"), ("C2", "C2V1"), ("C1", "C1V2"));

        // Act
        Dictionary<string, string> claims = userClaimsResolver.Resolve(user, default, default, default);

        // Assert
        Assert.Equal(2, claims.Count);

        KeyValuePair<string, string> c1Claim = Assert.Single(claims.Where(kv => kv.Key == "C1"));
        List<string> c1ClaimValues = JsonSerializer.Deserialize<List<string>>(c1Claim.Value);
        Assert.All(new[] { "C1V1", "C1V2" }, v => c1ClaimValues.Contains(v));

        KeyValuePair<string, string> c2Claim = Assert.Single(claims.Where(kv => kv.Key == "C2"));
        Assert.Equal("C2V1", c2Claim.Value);
    }

    private static async Task<(UserClaimsResolver, User)> CreateResolverAsync(params (string, string)[] claims)
    {
        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        UserManager<User> userManager = ServicesFactory.GetUserManager(dataContext);

        User user = new() { UserName = "Test User" };
        await userManager.CreateAsync(user);
        await userManager.AddClaimsAsync(user, claims.Select(kv => new Claim(kv.Item1, kv.Item2)));

        return (new(userManager), user);
    }
}