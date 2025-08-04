using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.ModuleLinkage;
using BBWM.Core.Test;
using BBWM.SystemSettings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BBWM.Core.Membership.Test.ModuleLinkage;

public class DataModuleLinkageTests
{
    private IMapper _mapper;
    private DataContext _context;

    public DataModuleLinkageTests()
    {
        _mapper = AutoMapperConfig.CreateMapper();
        _context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
    }

    [Fact]
    public async Task Ensure_Initial_Data_Test()
    {

        var roleManager = ServicesFactory.GetRoleManager(_context);
        await roleManager.CreateAsync(new Role("SuperAdmin2"));
        await roleManager.CreateAsync(new Role("SuperAdminRole"));

        var userInitService = new Mock<IUserInitializeService>();
        var settingService = new SettingsService(_context, new SettingsSectionService(), null);

        var provider = new Mock<IServiceProvider>();
        provider.Setup(p => p.GetService(typeof(RoleManager<Role>))).Returns(roleManager);
        provider.Setup(p => p.GetService(typeof(IDbContext))).Returns(_context);
        provider.Setup(p => p.GetService(typeof(IUserInitializeService))).Returns(userInitService.Object);
        provider.Setup(p => p.GetService(typeof(ISettingsService))).Returns(settingService);

        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider).Returns(provider.Object);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory.Setup(p => p.CreateScope()).Returns(serviceScope.Object);
        provider.Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactory.Object);

        var task = new DataModuleLinkage();

        await task.EnsureInitialData(serviceScope.Object, true);

        Assert.IsType<DataModuleLinkage>(task);
        Assert.NotNull(provider.Object);
        Assert.NotNull(serviceScope.Object);
        Assert.NotNull(serviceScopeFactory.Object);
        Assert.NotEmpty(roleManager.Roles);
    }
}
