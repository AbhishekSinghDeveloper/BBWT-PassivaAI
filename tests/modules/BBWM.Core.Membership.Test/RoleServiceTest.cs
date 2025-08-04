using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Membership.Utils;
using BBWM.Core.ModelHashing;
using BBWM.Core.Services;
using BBWM.Core.Test;
using BBWM.Core.Test.Fixtures;
using BBWM.Core.Test.Utils;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BBWM.Core.Membership.Test;

public class RoleServiceTest : IClassFixture<MappingFixture>
{
    private const string TestRole = "TestRole";

    private IMapper Mapper { get; }

    public RoleServiceTest(MappingFixture mappingFixture)
        => Mapper = mappingFixture.DefaultMapper;

    [Fact]
    public async Task GetEntityQuery()
    {
        // Arrange
        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        Role role = await InsertTestRoleAsync(dataContext);

        RoleService roleService = new(
            dataContext,
            Mock.Of<IMapper>(),
            Mock.Of<IDataService>(),
            ServicesFactory.GetRoleManager(dataContext),
            Mock.Of<IModelHashingService>());

        // Act
        IQueryable<Role> query = roleService.GetEntityQuery(dataContext.Roles);

        // Assert
        Role queriedRole = Assert.Single(await query.ToListAsync());
        Assert.Equal(role.Id, queriedRole.Id);
        Assert.Equal(role.Name, queriedRole.Name);
    }

    [Fact]
    public async Task GetHardcodedRoles()
    {
        // Arrange
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        IRoleService roleService = GetService(dataContext);

        // Act
        await InsertHardCodedRoles(roleService);
        await roleService.CleanupRoles();
        var hardCodedRoles = roleService.GetHardcodedRoles();

        // Assert
        Assert.NotEmpty(hardCodedRoles);
    }

    [Fact]
    public async Task GetProjectRoles()
    {
        // Arrange
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        IRoleService roleService = GetService(dataContext);

        await InsertTestRoleAsync(dataContext);
        await InsertHardCodedRoles(roleService);

        // Act
        IEnumerable<RoleDTO> roles = roleService.GetProjectRoles();

        // Assert
        RoleDTO role = Assert.Single(roles);
        Assert.Equal(TestRole, role.Name);
    }

    [Fact]
    public async Task Create_Should_Throw_On_Duplicate_Role()
    {
        // Arrange
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (roleService, roleManager) = GetServices(dataContext);

        await roleManager.CreateAsync(new() { Name = TestRole });

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => roleService.Create(new() { Name = TestRole }));
    }

    [Fact]
    public async Task Create()
    {
        // Arrange
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (roleService, roleManager) = GetServices(dataContext);

        // Act
        await roleService.Create(new() { Name = TestRole });

        // Assert
        Role role = Assert.Single(roleManager.Roles);
        Assert.Equal(TestRole, role.Name);
    }

    [Fact]
    public async Task Update_Should_Throw_On_Invalid_Name()
    {
        // Arrange
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (roleService, roleManager) = GetServices(dataContext);

        await roleManager.CreateAsync(new() { Name = TestRole });
        await roleManager.CreateAsync(new() { Name = "ToBeUpdated" });

        RoleDTO roleDTO = Mapper.Map<RoleDTO>(await roleManager.FindByNameAsync("ToBeUpdated"));
        roleDTO.Name = TestRole;

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => roleService.Update(roleDTO));
    }

    [Fact]
    public async Task Update_Should_Throw_On_Missing_Role()
    {
        // Arrange
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var roleService = GetService(dataContext);

        RoleDTO roleDTO = new() { Id = "Abc123", Name = TestRole };

        // Act & Assert
        await Assert.ThrowsAsync<ObjectNotExistsException>(() => roleService.Update(roleDTO));
    }

    [Fact]
    public async Task Update()
    {
        // Arrange
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (roleService, roleManager) = GetServices(dataContext);

        await roleManager.CreateAsync(new() { Name = TestRole });
        RoleDTO roleDTO = Mapper.Map<RoleDTO>(await roleManager.FindByNameAsync(TestRole));
        roleDTO.Name += " - updated!";

        // Act
        await roleService.Update(roleDTO);

        // Assert
        Role role = await roleManager.FindByIdAsync(roleDTO.Id);
        Assert.NotNull(role);
        Assert.Equal(roleDTO.Name, role.Name);
    }

    [Fact]
    public async Task Delete_Should_Throw_On_Using_Role()
    {
        // Arrange
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (roleService, roleManager) = GetServices(dataContext);

        Role role = new() { Name = TestRole };
        await roleManager.CreateAsync(role);
        await dataContext.UserRoles.AddAsync(new() { Role = role, User = new() });
        await dataContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => roleService.Delete(role.Id));
    }

    [Fact]
    public async Task Delete_Should_Throw_On_Missing_Role()
    {
        // Arrange
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var roleService = GetService(dataContext);

        // Act & Assert
        await Assert.ThrowsAsync<ObjectNotExistsException>(() => roleService.Delete("Abc123"));
    }

    [Fact]
    public async Task Delete()
    {
        // Arrange
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (roleService, roleManager) = GetServices(dataContext);

        Role role = new() { Name = TestRole };
        await roleManager.CreateAsync(role);

        // Act
        await roleService.Delete(role.Id);

        // Assert
        Assert.Empty(roleManager.Roles);
    }

    [Fact]
    public async Task CleanupRoles()
    {
        // Arrange
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        var (roleService, roleManager) = GetServices(dataContext);

        await InsertHardCodedRoles(roleService);

        Role role = new() { Name = TestRole };
        await roleManager.CreateAsync(role);

        // Act
        await roleService.CleanupRoles();

        // Assert
        int hardCodedRolesCount = RolesExtractor.GetAllRolesNamesOfSolution().ToHashSet().Count;
        Assert.Equal(hardCodedRolesCount, await roleManager.Roles.CountAsync());
    }

    [Fact]
    public async Task GetPage()
    {
        // Arrange
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();

        const string CanDoPermission = "CanDoXYZ";
        Permission permission = new() { Name = CanDoPermission };
        await dataContext.Set<Permission>().AddAsync(permission);
        await dataContext.SaveChangesAsync();

        Mock<IModelHashingService> modelHashingService = new();
        modelHashingService
            .Setup(h => h.UnHashProperty(typeof(PermissionDTO), nameof(PermissionDTO.Id), It.IsAny<string>()))
            .Returns(permission.Id);

        var (roleService, roleManager) = GetServices(dataContext, modelHashingService.Object);

        Role role = new() { Name = TestRole, RolePermissions = new[] { new RolePermission { Permission = permission } } };
        await roleManager.CreateAsync(role);

        QueryCommand queryCommand = new()
        {
            Filters = new()
            {
                new StringFilter
                {
                    MatchMode = StringFilterMatchMode.Equals,
                    PropertyName = "permissions",
                    Value = $"{permission.Id}-ABC12345",
                },
            },
        };

        // Act
        PageResult<RoleDTO> page = await roleService.GetPage(queryCommand);

        // Assert
        RoleDTO roleDTO = Assert.Single(page?.Items);
        Assert.Equal(role.Id, roleDTO.Id);
        Assert.Equal(TestRole, roleDTO.Name);

        PermissionDTO permissionDTO = Assert.Single(roleDTO.Permissions);
        Assert.Equal(permission.Id, permissionDTO.Id);
        Assert.Equal(CanDoPermission, permission.Name);
    }

    private async Task<Role> InsertTestRoleAsync(DataContext dataContext)
    {
        Role role = new() { Name = TestRole };
        await dataContext.Roles.AddAsync(role);
        await dataContext.SaveChangesAsync();

        return role;
    }

    private static async Task InsertHardCodedRoles(IRoleService roleService)
    {
        var hardCodedRoleNames = RolesExtractor.GetAllRolesNamesOfSolution().ToHashSet();

        foreach (var hdRoleName in hardCodedRoleNames)
        {
            await roleService.Create(new RoleDTO() { Name = hdRoleName }, CancellationToken.None);
        }
    }

    private IRoleService GetService(DataContext dataContext, IModelHashingService modelHashingService = default)
    {
        var (service, _) = GetServices(dataContext, modelHashingService);
        return service;
    }

    private (IRoleService, RoleManager<Role>) GetServices(DataContext dataContext, IModelHashingService modelHashingService = default)
    {
        var roleManager = ServicesFactory.GetRoleManager(dataContext);

        if (modelHashingService is null)
        {
            modelHashingService = new ModelHashingService();
            modelHashingService.Register(Mapper, dataContext);
        }

        var dataService = SutDataHelper.CreateEmptyDataService(Mapper, ctx: (IDbContext)dataContext);

        return (
            new RoleService(dataContext, Mapper, dataService, roleManager, modelHashingService),
            roleManager);
    }

    private static RoleDTO GetRoleDTOEntity()
    {
        var id = 0;
        var permission = new Faker<PermissionDTO>()
            .RuleFor(s => s.Id, c => ++id)
            .RuleFor(s => s.Name, c => c.Random.AlphaNumeric(10))
            .Generate(5);

        var faker = new Faker<RoleDTO>()
            .RuleFor(p => p.Name, s => s.Random.AlphaNumeric(10))
            .RuleFor(p => p.AuthenticatorRequired, s => false)
            .RuleFor(p => p.CheckIp, s => false)
            .RuleFor(p => p.Permissions, s => permission);

        return faker.Generate();
    }

    private RoleDTO GetRoleDTO()
    {
        var faker = new Faker<RoleDTO>()
            .RuleFor(p => p.Name, s => s.Random.AlphaNumeric(10))
            .RuleFor(p => p.AuthenticatorRequired, s => false)
            .RuleFor(p => p.CheckIp, s => false)
            .RuleFor(p => p.Permissions, s => new List<PermissionDTO>() { });

        return faker.Generate();
    }

    private static UserDTO GetUserDTOEntity()
    {
        var faker = new Faker<UserDTO>()
            .RuleFor(p => p.Id, s => "1")
            .RuleFor(p => p.FirstName, s => s.Person.FirstName)
            .RuleFor(p => p.LastName, s => s.Person.LastName)
            .RuleFor(p => p.Email, (s, p) => s.Internet.Email(p.FirstName, p.LastName))
            .RuleFor(p => p.UserName, (s, p) => p.Email)
            .RuleFor(p => p.Password, s => s.Internet.Password())
            .RuleFor(p => p.ConfirmPassword, (s, p) => p.Password)
            .RuleFor(p => p.TwoFactorEnabled, p => false);

        return faker.Generate();
    }
}
