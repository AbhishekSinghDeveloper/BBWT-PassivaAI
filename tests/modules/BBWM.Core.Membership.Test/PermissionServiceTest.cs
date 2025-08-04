using AutoMapper;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Membership.Utils;
using BBWM.Core.Test;
using Bogus;
using Xunit;

namespace BBWM.Core.Membership.Test;

public class PermissionServiceTest
{
    private readonly IMapper _mapper;
    private readonly DataContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionServiceTest"/> class.
    /// </summary>
    public PermissionServiceTest()
    {
        _context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
        _mapper = AutoMapperConfig.CreateMapper();
    }

    private IPermissionService GetService()
    {
        return new PermissionService(_context, _mapper);
    }

    private static PermissionDTO GetEntity()
    {
        var faker = new Faker<PermissionDTO>()
            .RuleFor(p => p.Name, s => s.Random.AlphaNumeric(10));

        return faker.Generate();
    }

    private async Task InsertRandomPermissions()
    {
        var dbSet = _context.Set<Permission>();

        for (int i = 0; i < 3; i++)
        {
            var permissionDto = GetEntity();
            var permission = _mapper.Map<Permission>(permissionDto);
            await dbSet.AddAsync(permission);
        }

        await _context.SaveChangesAsync(CancellationToken.None);
    }

    private async Task<int> InsertExceptedPermissions()
    {
        var exceptPermissions = PermissionsExtractor.GetAllPermissionNamesOfSolution().ToArray();
        var dbSet = _context.Set<Permission>();

        foreach (var permissionName in exceptPermissions)
        {
            var newPermission = new Permission() { Name = permissionName };
            await dbSet.AddAsync(newPermission);
        }

        await _context.SaveChangesAsync(CancellationToken.None);
        return exceptPermissions.Count();
    }

    [Fact]
    public async Task Must_Have_Permissions()
    {
        // Arrange
        var permService = this.GetService();

        // Act
        await this.InsertRandomPermissions();
        var permList = await permService.GetAll(CancellationToken.None);

        // Assert
        Assert.NotEmpty(permList);
    }

    [Fact]
    public async Task Has_Excepted_Permissions_After_Cleanup()
    {
        // Arrange
        var permService = this.GetService();

        // Act
        var exceptedCount = await this.InsertExceptedPermissions();
        if (exceptedCount > 0)
        {
            await permService.CleanupPermissions(CancellationToken.None);
            var permList = await permService.GetAll(CancellationToken.None);

            // Assert
            Assert.NotEmpty(permList);
        }
    }

    [Fact]
    public async Task Must_Not_Have_Any_Permissions_After_Cleanup()
    {
        // Arrange
        var permService = this.GetService();

        // Act
        await this.InsertRandomPermissions();
        await permService.CleanupPermissions(CancellationToken.None);
        var permList = await permService.GetAll(CancellationToken.None);

        // Assert
        Assert.Empty(permList);
    }
}
