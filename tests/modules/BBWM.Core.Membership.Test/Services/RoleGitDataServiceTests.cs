using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Test;
using BBWM.Core.Test.Utils;
using BBWM.GitLab;
using BBWM.Metadata;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using Xunit;

namespace BBWM.Core.Membership.Test.Services;

public class RoleGitDataServiceTests
{
    private const string RolesGit =
        @"{
            ""lastUpdated"": ""2021-10-29T14:15:52"",
            ""roles"": [
                { ""id"": ""TestRole"", ""name"": ""TestRole"" }
            ]
        }";

    private const string RolesGitPermission =
        @"{
            ""lastUpdated"": ""2021-10-29T14:15:52"",
            ""roles"": [
                {
                    ""id"": ""TestRole"",
                    ""name"": ""TestRole"",
                    ""permissions"": [
                        ""ThisRoleCanDoXYZ""
                    ]
                }
            ]
        }";

    private const string RolesGitInvalid =
        @"{
            ""lastUpdated"": ""2021-10-29T14:15:52"",
        ";

    private const string TestRole = "TestRole";

    private const string CanDoPermission = "ThisRoleCanDoXYZ";

    private static readonly User user = new User
    {
        Email = "temp@testing.com",
    };

    [Theory]
    [MemberData(nameof(UpdateRolesFromJsonTestData))]
    public async Task UpdateRolesFromJson(
        string rolesJson, int expectedRolesCount, string expectedRole, MetadataDTO metadataDTO)
    {
        // Arrange
        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (service, _) = CreateService(dataContext, rolesJson, metadataDTO);

        // Act
        await service.UpdateRolesFromJson();

        // Assert
        Assert.Equal(expectedRolesCount, await dataContext.Roles.CountAsync());
        if (!string.IsNullOrEmpty(expectedRole))
        {
            Role role = await dataContext.Roles.FirstOrDefaultAsync();
            Assert.Equal(expectedRole, role?.Id);
            Assert.Equal(expectedRole, role?.Name);
        }
    }

    [Fact]
    public async Task UpdateRolesFromJson_Should_Throw_On_Role_In_Use()
    {
        // Arrange
        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (service, _) = CreateService(dataContext, RolesGit);

        await dataContext.Roles.AddAsync(new() { Id = "SuperAdminWrong", Name = "SuperAdminWrong" });
        await dataContext.UserRoles.AddAsync(new() { RoleId = "SuperAdminWrong", User = new User() });
        await dataContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateRolesFromJson());
    }

    [Fact]
    public async Task UpdateRolesFromJson_Should_Delete_Existent_Role()
    {
        // Arrange
        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (service, _) = CreateService(dataContext, RolesGit);

        await dataContext.Roles.AddAsync(new() { Name = "SuperAdminOld" });
        await dataContext.SaveChangesAsync();

        // Act
        await service.UpdateRolesFromJson();

        // Assert
        Assert.Equal(1, await dataContext.Roles.CountAsync());
        Role role = await dataContext.Roles.FirstOrDefaultAsync();
        Assert.Equal(TestRole, role?.Id);
        Assert.Equal(TestRole, role?.Name);
    }

    [Fact]
    public async Task UpdateRolesFromJson_Should_Not_Delete_Existent_Role()
    {
        // Arrange
        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (service, _) = CreateService(dataContext, RolesGit);

        await dataContext.Roles.AddAsync(new() { Id = TestRole, Name = TestRole });
        await dataContext.SaveChangesAsync();

        // Act
        await service.UpdateRolesFromJson();

        // Assert
        Assert.Equal(1, await dataContext.Roles.CountAsync());
        Role role = await dataContext.Roles.FirstOrDefaultAsync();
        Assert.Equal(TestRole, role?.Id);
        Assert.Equal(TestRole, role?.Name);
    }

    [Fact]
    public async Task UpdateRolesFromJson_Should_Throw_On_Missing_Permission()
    {
        // Arrange
        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (service, _) = CreateService(dataContext, RolesGitPermission);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateRolesFromJson());
    }

    [Fact]
    public async Task UpdateRolesFromJson_Should_Create_Role_Permission()
    {
        // Arrange
        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (service, _) = CreateService(dataContext, RolesGitPermission);

        Permission permission = new() { Name = CanDoPermission };
        await dataContext.Set<Permission>().AddAsync(permission);
        await dataContext.SaveChangesAsync();

        // Act
        await service.UpdateRolesFromJson();

        // Assert
        RolePermission rolePermission = Assert.Single(
            dataContext.Set<RolePermission>()
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == TestRole && rp.PermissionId == permission.Id));
        Assert.Equal(TestRole, rolePermission.Role?.Name);
        Assert.Equal(CanDoPermission, rolePermission.Permission?.Name);
    }

    [Fact]
    public async Task SendToGit()
    {
        // Arrange
        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (service, gitlabService) = CreateService(dataContext, RolesGit);

        RolePermission rolePermission = new()
        {
            Role = new() { Name = "SuperAdminOld" },
            Permission = new() { Name = CanDoPermission },
        };
        await dataContext.Set<RolePermission>().AddAsync(rolePermission);
        await dataContext.SaveChangesAsync();

        // Act
        await service.SendToGit();

        // Assert
        gitlabService.Verify();
    }

    [Fact]
    public async Task SendToGit_Should_Throw_On_Missing_User()
    {
        // Arrange
        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (service, gitlabService) = CreateService(dataContext, RolesGit);
        dataContext.Users.RemoveRange(dataContext.Users);
        await dataContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.SendToGit());
    }

    public static IEnumerable<object[]> UpdateRolesFromJsonTestData => new[]
    {
        new object[] { RolesGit, 1, TestRole, null },
        new object[] { "null", 0, null, null },
        new object[]
        {
            RolesGitInvalid,
            0,
            null,
            new MetadataDTO { LastUpdated = new(2020, 1, 1, 0, 0, 0, TimeSpan.Zero) },
        },
    };

    private static (IRoleGitDataService, Mock<IGitLabService>) CreateService(
        DataContext context, string rolesJson, MetadataDTO metadataDTO = default)
    {
        var tmpFile = Path.GetTempFileName();
        File.WriteAllText(tmpFile, rolesJson);
        var contentRoot = Path.GetDirectoryName(tmpFile);
        if (!contentRoot.EndsWith(Path.DirectorySeparatorChar))
            contentRoot += Path.DirectorySeparatorChar;

        var membershipSettings = new MembershipSettings();
        membershipSettings.RolesFilePath = Path.GetFileName(tmpFile);
        var mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        mockWebHostEnvironment.Setup(p => p.ContentRootPath).Returns(contentRoot);
        mockWebHostEnvironment.Setup(p => p.EnvironmentName).Returns("UnitTesting");
        var mockMetadataService = new Mock<IMetadataService>();
        mockMetadataService.Setup(m => m.Save(It.IsAny<MetadataDTO>())).Returns<MetadataDTO>(m => m);
        if (metadataDTO is not null)
            mockMetadataService.Setup(m => m.GetByKey(It.IsAny<string>())).Returns(metadataDTO);

        var mockGitLabService = new Mock<IGitLabService>();
        mockGitLabService
            .Setup(g => g.Push(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Verifiable();

        var mockOptionsSnapshot = new Mock<IOptionsSnapshot<MembershipSettings>>();
        mockOptionsSnapshot.Setup(p => p.Value).Returns(membershipSettings);

        if (context.Users.FirstOrDefault(u => u.Email == user.Email) is null)
        {
            context.Users.Add(user);
            context.SaveChanges();
        }

        var contextAccessor = Core.Test.ServicesFactory.GetHttpContextAccessor(
            new List<Claim> { new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id) });

        return (
            new RoleGitDataService(
                context,
                mockWebHostEnvironment.Object,
                contextAccessor,
                ServicesFactory.GetUserManager(context),
                mockMetadataService.Object,
                mockGitLabService.Object,
                mockOptionsSnapshot.Object),
            mockGitLabService);
    }
}
