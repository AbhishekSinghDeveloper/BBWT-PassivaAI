using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.ModelHashing;
using BBWM.Core.Test;
using BBWM.SystemSettings;
using Xunit;

namespace BBWM.Core.Membership.Test;

public class UserInitializeServiceTest
{
    private readonly IMapper _mapper;

    public UserInitializeServiceTest()
    {
        _mapper = AutoMapperConfig.CreateMapper();
    }

    protected IDbContext GetContext()
    {
        return InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
    }

    protected IUserInitializeService GetService<TContext>(TContext context)
    {
        if (!(context is DataContext ctx))
        {
            throw new InvalidCastException();
        }

        var userManager = ServicesFactory.GetUserManager(ctx);
        var settingService = new SettingsService(ctx, new SettingsSectionService(), null);

        // modelHashingService registered as a singleton
        var modelHashingService = new ModelHashingService();
        modelHashingService.Register(_mapper, ctx);

        var securityService = new SecurityService(ctx, Core.Test.ServicesFactory.GetHttpContextAccessor(), userManager, settingService, null);

        return new UserInitializeService(
            userManager,
            securityService,
            ctx,
            _mapper);
    }

    [Fact]
    public async Task CreateInitialUserTest()
    {
        using (var ctx = GetContext())
        {
            // Arrange
            var sut = GetService(ctx);
            var dto = new UserDTO { Email = "test@email.com", Password = "P@ssword1" };
            string roleName = "test role";
            var roleManager = ServicesFactory.GetRoleManager(ctx as DataContext);
            await roleManager.CreateAsync(new Role(roleName));
            string permissionName = "test permission";
            ctx.Set<Permission>().Add(new Permission { Name = permissionName });
            string groupName = "test group";
            ctx.Set<Group>().Add(new Group { Name = groupName });
            ctx.SaveChanges();

            // Act
            await sut.CreateInitialUser(dto, null, new string[] { permissionName }, new string[] { groupName });
            await sut.CreateInitialUser(dto, permissionName);
            await sut.CreateInitialUser(dto, new string[] { permissionName });
            var result = ctx.Set<User>().FirstOrDefault();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Email, result.Email);
            //Assert.Contains(result.UserRoles, r => r.Role.Name.Equals(roleName));
            Assert.Contains(result.UserPermissions, p => p.Permission.Name.Equals(permissionName));
            Assert.Contains(result.UserGroups, g => g.Group.Name.Equals(groupName));
        }
    }
}
