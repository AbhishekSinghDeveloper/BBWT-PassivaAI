using AutoMapper;
using BBWM.Core.Membership.Authorization;
using BBWM.Core.Membership.Constants;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Test;
using Bogus;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using Xunit;

namespace BBWM.Core.Membership.Test.Authorization;

public class AuditableSignInManagerTests
{
    private readonly IMapper _mapper;
    private readonly DataContext _context;

    public AuditableSignInManagerTests()
    {
        _context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
        _mapper = AutoMapperConfig.CreateMapper();
    }

    private static AuditableSignInManager GetService<TContext>(TContext context)
    {
        if (context is not DataContext ctx)
        {
            throw new InvalidCastException();
        }

        var mockAuthenticationSchemeProvider = new Mock<IAuthenticationSchemeProvider>();
        var mockUserClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
        var mockOptions = new Mock<IOptions<IdentityOptions>>();
        var mockUserConfirmation = new Mock<IUserConfirmation<User>>();
        var mockLogger = new Mock<ILogger<AuditableSignInManager>>();

        var authService = new Mock<IAuthenticationService>();
        authService
            .Setup(s => s.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(c => c.HttpContext.User).Returns(new ClaimsPrincipal());

        var headers = new HeaderDictionary()
        {
            ["X-Browser-Id"] = "Edge 95.0.1020.44",
            ["X-Browser-Fingerprint"] = "1885987160",
        };
        mockHttpContextAccessor
            .Setup(c => c.HttpContext.RequestServices.GetService(typeof(IAuthenticationService)))
            .Returns(authService.Object);
        mockHttpContextAccessor
            .Setup(c => c.HttpContext.Request.Headers)
            .Returns(headers);

        var auditService = new LoginAuditService(mockHttpContextAccessor.Object, Mock.Of<ILogger<LoginAuditService>>(), ctx);

        var userManager = ServicesFactory.GetUserManager(ctx);

        return new AuditableSignInManager(
            mockAuthenticationSchemeProvider.Object,
            mockUserClaimsPrincipalFactory.Object,
            mockOptions.Object,
            mockUserConfirmation.Object,
            mockLogger.Object,
            auditService,
            mockHttpContextAccessor.Object,
            userManager);
    }

    private static UserDTO GetUserDto()
    {
        var faker = new Faker<UserDTO>()
            .RuleFor(p => p.Id, s => null)
            .RuleFor(p => p.FirstName, s => s.Person.FirstName)
            .RuleFor(p => p.LastName, s => s.Person.LastName)
            .RuleFor(p => p.Email, (s, p) => s.Internet.Email(p.FirstName, p.LastName))
            .RuleFor(p => p.UserName, (s, p) => p.Email)
            .RuleFor(p => p.Password, s => "password")
            .RuleFor(p => p.ConfirmPassword, (s, p) => p.Password)
            .RuleFor(p => p.TwoFactorEnabled, p => false)
            .RuleFor(p => p.Roles, s => new List<RoleDTO>() { })
            .RuleFor(p => p.Groups, s => new List<GroupDTO>() { });

        return faker.Generate();
    }

    [Fact]
    public async Task Password_Sign_In_Async_Test()
    {
        // Arrange
        var service = GetService(_context);
        var userDto = GetUserDto();
        var userMapper = _mapper.Map<User>(userDto);

        _context.Set<User>().Add(userMapper);
        _context.SaveChanges();

        // Act
        var passwordSingInAsyncResult = await service.PasswordSignInAsync(userMapper, "password", true, true);

        // Assert
        Assert.False(passwordSingInAsyncResult.Succeeded);
        var audit = Assert.Single(_context.Set<LoginAudit>());
        Assert.Equal(audit.Email, userDto.Email);
        Assert.Equal(audit.Result, LogMessages.FailedToLogin);
    }

    [Fact]
    public async Task Sign_In_Async_Test()
    {
        // Arrange
        var service = GetService(_context);
        var userDto = GetUserDto();
        var userMapper = _mapper.Map<User>(userDto);

        _context.Set<User>().Add(userMapper);
        _context.SaveChanges();

        service.Context.User.AddIdentity(
            new ClaimsIdentity(new[] { new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userMapper.Id) }));

        // Act
        await service.SignInAsync(userMapper, new AuthenticationProperties());
        await service.SignOutAsync();

        // Assert
        var audits = await _context.Set<LoginAudit>().ToListAsync();
        Assert.Subset(
            audits.Select(a => a.Result).ToHashSet(),
            new HashSet<string> { LogMessages.LoggedIn, LogMessages.SignedOut });
        Assert.All(audits, (audit) => Assert.Equal(audit.Email, userDto.Email));
    }

    [Fact]
    public async Task Two_Factor_Sign_In_Async_Test()
    {
        // Arrange
        var service = GetService(_context);
        var userDto = GetUserDto();
        var userMapper = _mapper.Map<User>(userDto);

        _context.Set<User>().Add(userMapper);
        _context.SaveChanges();

        // Act
        var twoFactorAuthSigninAsyncResult =
            await service.TwoFactorAuthenticatorSignInAsync("code", false, false);

        // Assert
        Assert.False(twoFactorAuthSigninAsyncResult.Succeeded);
    }
}
