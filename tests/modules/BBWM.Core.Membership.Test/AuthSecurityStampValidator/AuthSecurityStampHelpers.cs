using BBWM.Core.Membership.Model;
using BBWM.Core.Test;
using BBWM.Core.Test.Utils;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using System.Security.Claims;

using AspNetClaimTypes = System.Security.Claims.ClaimTypes;
using BbwtClaimTypes = BBWM.Core.Membership.Model.ClaimTypes;

namespace BBWM.Core.Membership.Test.AuthSecurityStampValidator;

public enum ValidationIntervalCreateMode
{
    WithNullValue = 1,
    WithValue,
}

internal static class AuthSecurityStampHelpers
{
    internal static async Task<DataContext> CreateDataContext(User user)
    {
        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        if (user is not null)
        {
            await dataContext.Users.AddAsync(user);
            await dataContext.SaveChangesAsync();
        }

        return dataContext;
    }

    internal static IOptions<AuthSecurityStampValidatorOptions> CreateOptions(
        TimeSpan authValidationInterval, ValidationIntervalCreateMode intervalCreateMode)
    {
        Mock<IOptions<AuthSecurityStampValidatorOptions>> options = new();

        switch (intervalCreateMode)
        {
            case ValidationIntervalCreateMode.WithNullValue:
                options.Setup(opts => opts.Value).Returns((AuthSecurityStampValidatorOptions)null);
                break;
            case ValidationIntervalCreateMode.WithValue:
                AuthSecurityStampValidatorOptions authSecurityStampValidatorOptions = new()
                {
                    AuthValidationInterval = authValidationInterval,
                    ValidationInterval = TimeSpan.FromMinutes(10),
                };

                options.Setup(opts => opts.Value).Returns(authSecurityStampValidatorOptions);
                break;
        }

        return options.Object;
    }

    internal static IHttpContextAccessor CreateHttpContextAccessor(
        string userId, string userName, string authSecurityStamp)
    {
        List<Claim> userClaims = new();

        if (userId is not null)
            userClaims.Add(new(AspNetClaimTypes.NameIdentifier, userId));

        if (userName is not null)
            userClaims.Add(new(AspNetClaimTypes.Name, userName));

        if (authSecurityStamp is not null)
            userClaims.Add(new(BbwtClaimTypes.Authentication.AuthSecurityStamp, authSecurityStamp));

        IHttpContextAccessor httpContextAccessor = Core.Test.ServicesFactory.GetHttpContextAccessor(userClaims);

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IAuthenticationService>());

        httpContextAccessor.HttpContext.RequestServices = services.BuildServiceProvider();

        return httpContextAccessor;
    }

    internal static ISystemClock CreateClock()
    {
        Mock<ISystemClock> clock = new();
        clock.Setup(c => c.UtcNow).Returns(DateTimeOffset.UtcNow);

        return clock.Object;
    }

    internal static ILoggerFactory CreateLoggerFactory()
    {
        Mock<ILoggerFactory> loggerFactory = new();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());

        return loggerFactory.Object;
    }

    internal static CookieValidatePrincipalContext CreateCookieValidatePrincipalContext(
        HttpContext httpContext, DateTimeOffset? issuedUtc)
    {
        AuthenticationProperties authenticationProperties = new() { IssuedUtc = issuedUtc };

        AuthenticationTicket authenticationTicket = new(httpContext.User, authenticationProperties, "mock");

        return new(
            httpContext,
            new("mock", "mock", typeof(NoopHandler)),
            new(),
            authenticationTicket);
    }

    private class NoopHandler : IAuthenticationHandler
    {
        public Task<AuthenticateResult> AuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            throw new NotImplementedException();
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            throw new NotImplementedException();
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
