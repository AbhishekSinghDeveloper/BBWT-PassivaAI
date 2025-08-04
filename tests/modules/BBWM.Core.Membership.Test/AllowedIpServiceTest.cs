using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Test;
using Xunit;

namespace BBWM.Core.Membership.Test;

public class AllowedIpServiceTest
{
    private readonly DataContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedIpServiceTest"/> class.
    /// </summary>
    public AllowedIpServiceTest()
    {
        _context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
    }

    private IAllowedIpService GetService()
        => new AllowedIpService(_context, ServicesFactory.GetUserManager(_context));

    [Fact]
    public async Task Is_Allowed_Ip_Service_Active()
    {
        // Arrange
        var sut = GetService();

        var user = new User()
        {
            AllowedIpUser = new List<AllowedIpUser>(),
            UserRoles = this.GetUserRoles("192.168.12.200", "192.168.12.255"),
        };
        await _context.Set<User>().AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var isActive = await sut.IsServiceActive(CancellationToken.None);

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public async Task Ip_Must_Be_Allowed_For_User_By_AllowedUserIpRanges()
    {
        // Arrange
        var sut = GetService();
        var ip = "192.168.12.250";

        var user = new User()
        {
            AllowedIpUser = this.GetAllowedIpUsers("192.168.12.200", "192.168.12.255"),
            UserRoles = new List<UserRole>(),
        };
        await _context.Set<User>().AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var isIpAllowed = await sut.IsIpAllowedForUser(ip, user.Id, CancellationToken.None);

        // Assert
        Assert.True(isIpAllowed);
    }

    [Fact]
    public async Task Ip_Must_Be_Allowed_For_User_By_AllowedUserRolesIpRanges()
    {
        // Arrange
        var sut = GetService();
        var ip = "192.168.12.250";

        var user = new User()
        {
            AllowedIpUser = new List<AllowedIpUser>(),
            UserRoles = this.GetUserRoles("192.168.12.200", "192.168.12.255"),
        };
        await _context.Set<User>().AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var isIpAllowed = await sut.IsIpAllowedForUser(ip, user.Id, CancellationToken.None);

        // Assert
        Assert.True(isIpAllowed);
    }

    [Fact]
    public async Task Ip_Must_NOT_Be_Allowed_For_User()
    {
        // Arrange
        var sut = GetService();
        var ip = "192.168.12.100";

        var user = new User()
        {
            AllowedIpUser = this.GetAllowedIpUsers("192.168.12.200", "192.168.12.255"),
            UserRoles = this.GetUserRoles("192.168.12.200", "192.168.12.255"),
        };
        await _context.Set<User>().AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var isIpAllowed = await sut.IsIpAllowedForUser(ip, user.Id, CancellationToken.None);

        // Assert
        Assert.False(isIpAllowed);
    }

    private List<UserRole> GetUserRoles(string ipAddressFirst, string ipAddressLast)
    {
        return new List<UserRole>() {
                new UserRole()
                {
                    Role = new Role()
                    {
                        AllowedIpRoles = new List<AllowedIpRole>()
                        {
                            new AllowedIpRole()
                            {
                                AllowedIp = new AllowedIp()
                                {
                                    IpAddressFirst = ipAddressFirst,
                                    IpAddressLast = ipAddressLast,
                                },
                            },
                        },
                    },
                },
             };
    }

    private List<AllowedIpUser> GetAllowedIpUsers(string ipAddressFirst, string ipAddressLast)
    {
        return new List<AllowedIpUser>()
            {
                new AllowedIpUser()
                {
                    AllowedIp = new AllowedIp()
                    {
                        IpAddressFirst = ipAddressFirst,
                        IpAddressLast = ipAddressLast,
                    },
                },
            };
    }
}
