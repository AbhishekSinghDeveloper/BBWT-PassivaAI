using AutoMapper;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Services;
using BBWM.Core.Test;
using BBWM.Core.Test.Fixtures;
using BBWM.Core.Test.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MembershipServicesFactory = BBWM.Core.Membership.Test.ServicesFactory;

namespace BBWM.Core.Membership.Test.Services;

public class AllowIpServiceTests : IClassFixture<MappingFixture>
{
    private const string RoleOne = "role-one";
    private const string RoleTwo = "role-two";
    private const string RoleThree = "role-three";

    private const string UserNameOne = "User One";
    private const string UserNameTwo = "User Two";

    public AllowIpServiceTests(MappingFixture mappingFixture)
    {
        Mapper = mappingFixture.DefaultMapper;
    }

    public IMapper Mapper { get; }

    [Fact]
    public async Task Should_Create_Allowed_Ip()
    {
        // Arrange
        CreateServiceResult srvResult = await CreateServiceAsync();

        RoleDTO roleOneDTO = srvResult.GetRoleDTOAt(0);
        RoleDTO roleTwoDTO = srvResult.GetRoleDTOAt(1);
        UserDTO userDTO = srvResult.UserDTO;

        // Act
        AllowedIpDTO allowedIpDTO = await srvResult.IpService.Create(
            new()
            {
                Roles = new() { roleOneDTO, roleTwoDTO },
                Users = new() { userDTO },
            });

        // Assert
        string[] expectedRoleIds = new[] { srvResult.Roles[0].Id, srvResult.Roles[1].Id };
        AssertAllowedIpDTO(allowedIpDTO, srvResult.User.Id, expectedRoleIds);
        await AssertAllowedIp(srvResult.DataContext, allowedIpDTO.Id, srvResult.User.Id, expectedRoleIds);
    }

    [Fact]
    public async Task Should_Update_Allowed_Ip()
    {
        // Arrange
        CreateServiceResult srvResult = await CreateServiceAsync();

        AllowedIp allowedIp = await CreateAllowedIp(srvResult);

        RoleDTO roleOneDTO = srvResult.GetRoleDTOAt(0);
        RoleDTO roleTwoDTO = srvResult.GetRoleDTOAt(1);
        UserDTO userDTO = srvResult.UserDTO;

        // Act
        AllowedIpDTO allowedIpDTO = await srvResult.IpService.Update(
            new()
            {
                Id = allowedIp.Id,
                Roles = new() { roleOneDTO, roleTwoDTO },
                Users = new() { userDTO },
            });

        // Assert
        string[] expectedRoleIds = new[] { srvResult.Roles[0].Id, srvResult.Roles[1].Id };
        AssertAllowedIpDTO(allowedIpDTO, srvResult.User.Id, expectedRoleIds);
        await AssertAllowedIp(srvResult.DataContext, allowedIp.Id, srvResult.User.Id, expectedRoleIds);
    }

    [Fact]
    public async Task GetEntityQuery_Should_Get_AllowedIp()
    {
        // Arrange
        CreateServiceResult srvResult = await CreateServiceAsync();
        AllowedIp allowedIp = await CreateAllowedIp(srvResult);

        srvResult.DataContext.ChangeTracker
            .Entries<AllowedIp>()
            .ToList()
            .ForEach(e => e.State = EntityState.Detached);

        IQueryable<AllowedIp> query = srvResult.IpService.GetEntityQuery(srvResult.DataContext.Set<AllowedIp>());
        User expectedUser = allowedIp.AllowedIpUsers.FirstOrDefault()?.User;

        // Act
        List<AllowedIp> allowedIps = await query.Where(ip => ip.Id == allowedIp.Id).ToListAsync();

        // Assert
        AllowedIp allowedIpDb = Assert.Single(allowedIps);

        AllowedIpUser allowedIpUser = Assert.Single(allowedIpDb.AllowedIpUsers);
        Assert.NotNull(allowedIpUser.User);
        Assert.Equal(expectedUser.Id, allowedIpUser.User.Id);
        Assert.Equal(expectedUser.UserName, allowedIpUser.User.UserName);

        Assert.Equal(2, allowedIpDb.AllowedIpRoles.Count);
        Assert.All(
            new[] { srvResult.Roles[1], srvResult.Roles[2] },
            role =>
            {
                AllowedIpRole allowedIpRole = allowedIpDb.AllowedIpRoles.FirstOrDefault(ir => ir.RoleId == role.Id);
                Assert.NotNull(allowedIpRole?.Role);
                Assert.Equal(role.Name, allowedIpRole?.Role.Name);
            });
    }

    private static async Task<AllowedIp> CreateAllowedIp(CreateServiceResult srvResult)
    {
        AllowedIpRole CreateAllowedIpRole(int index) => new() { Role = srvResult.Roles[index] };

        AllowedIp allowedIp = new()
        {
            AllowedIpRoles = new List<AllowedIpRole> { CreateAllowedIpRole(1), CreateAllowedIpRole(2) },
            AllowedIpUsers = new List<AllowedIpUser> { new() { User = new() { UserName = UserNameTwo } } },
        };

        await srvResult.DataContext.Set<AllowedIp>().AddAsync(allowedIp);
        await srvResult.DataContext.SaveChangesAsync();
        return allowedIp;
    }

    private static void AssertAllowedIpDTO(
        AllowedIpDTO allowedIpDTO, string expectedUserId, string[] expectedRoleIds)
    {
        Assert.NotNull(allowedIpDTO);
        UserDTO allowedUserDTO = Assert.Single(allowedIpDTO.Users);
        Assert.Equal(expectedUserId, allowedUserDTO.Id);

        Assert.Equal(2, allowedIpDTO.Roles?.Count ?? 0);
        List<string> allowedRoleIds = allowedIpDTO.Roles.Select(r => r.Id).ToList();
        Assert.All(expectedRoleIds, roleId => Assert.Contains(roleId, allowedRoleIds));
    }

    private async Task<CreateServiceResult> CreateServiceAsync()
    {
        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        BbwtUserManager<User> userManager = MembershipServicesFactory.GetBbwtUserManager(dataContext);
        RoleManager<Role> roleManager = MembershipServicesFactory.GetRoleManager(dataContext);

        Role[] roles = new[] { RoleOne, RoleTwo, RoleThree }.Select(r => new Role { Name = r }).ToArray();

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        User user = new() { UserName = UserNameOne };
        await userManager.CreateAsync(user);

        return new(Mapper)
        {
            IpService = new(dataContext, new DataService(dataContext, Mapper)),
            Roles = roles,
            User = user,
            DataContext = dataContext,
            UserManager = userManager,
            RoleManager = roleManager,
        };
    }

    private static async Task AssertAllowedIp(
        DataContext dataContext, int allowedIpId, string expectedUserId, string[] expectedRoleIds)
    {
        var allowedIp = await dataContext.Set<AllowedIp>().FindAsync(allowedIpId);
        Assert.NotNull(allowedIp);

        var allowedIpUser = await dataContext.Set<AllowedIpUser>().FirstOrDefaultAsync(iu => iu.UserId == expectedUserId);
        Assert.NotNull(allowedIpUser);
        Assert.Equal(allowedIpId, allowedIpUser.AllowedIpId);

        var allowedIpRoles = await dataContext.Set<AllowedIpRole>()
            .Where(ir => expectedRoleIds.Contains(ir.RoleId))
            .ToListAsync();

        Assert.Equal(expectedRoleIds.Length, allowedIpRoles.Count);
        Assert.All(allowedIpRoles, ir => Assert.Equal(allowedIpId, ir.AllowedIpId));
    }

    private class CreateServiceResult
    {
        private readonly IMapper _mapper;

        public CreateServiceResult(IMapper mapper) => _mapper = mapper;

        public AllowedIpDataService IpService { get; set; }

        public Role[] Roles { get; set; }

        public User User { get; set; }

        public UserDTO UserDTO => _mapper.Map<UserDTO>(User);

        public DataContext DataContext { get; set; }

        public BbwtUserManager<User> UserManager { get; set; }

        public RoleManager<Role> RoleManager { get; set; }

        public RoleDTO GetRoleDTOAt(int index) => _mapper.Map<RoleDTO>(Roles[index]);
    }
}
