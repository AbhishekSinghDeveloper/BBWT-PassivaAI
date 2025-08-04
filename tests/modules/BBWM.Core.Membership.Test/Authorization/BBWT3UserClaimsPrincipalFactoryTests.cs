using AutoMapper;

using BBWM.Core.Membership.Authorization;
using BBWM.Core.Test;
using BBWM.SystemSettings;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace BBWM.Core.Membership.Test.Authorization;

public class BBWT3UserClaimsPrincipalFactoryTests
{
    private readonly IMapper _mapper;
    private readonly DataContext _context;

    public BBWT3UserClaimsPrincipalFactoryTests()
    {
        _context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
        _mapper = AutoMapperConfig.CreateMapper();
    }

    private static BBWT3UserClaimsPrincipalFactory GetService<TContext>(TContext context)
    {
        if (context is not DataContext ctx)
        {
            throw new InvalidCastException();
        }

        var userManager = ServicesFactory.GetUserManager(ctx);
        var mockRoleManager = ServicesFactory.GetRoleManager(ctx);
        var mockOptions = new Mock<IOptions<IdentityOptions>>();
        mockOptions.Setup(p => p.Value).Returns(new IdentityOptions());
        var mockSettingsService = new Mock<ISettingsService>();

        return new BBWT3UserClaimsPrincipalFactory(
            userManager,
            mockRoleManager,
            mockOptions.Object,
            mockSettingsService.Object);
    }

    [Fact]
    public async Task Generate_Claims_Async_Test()
    {
        var service = GetService(_context);
    }
}
