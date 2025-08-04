using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBWM.Core.Test;
using BBWM.Core.Test.Fixtures;
using BBWM.Core.Test.Utils;
using BBWM.GitLab;
using BBWM.Menu.Db;
using BBWM.Menu.DTO;
using BBWM.Menu.JsonGit;

using BBWT.Data;

using Bogus;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Moq;

using System.Security.Claims;

using Xunit;

namespace BBWM.Menu.Test;

public class FooterMenuServiceTests : IClassFixture<MappingFixture>
{
    private readonly IMapper mapper;

    public FooterMenuServiceTests(MappingFixture mappingFixture)
    {
        this.mapper = mappingFixture.DefaultMapper;
    }

    private DbFooterMenuDataProvider GetDbFooterService(IDataContext ctx)
    {
        return new DbFooterMenuDataProvider(ctx, this.mapper);
    }

    private (IWebHostEnvironment, Mock<IGitLabService>, IHttpContextAccessor, string) GetJsonGitMenuParams(IDbContext ctx)
    {
        var menuFullPath = Path.GetTempFileName();
        var path = Path.GetFileName(menuFullPath);

        var webHost = new Mock<IWebHostEnvironment>();
        webHost.Setup(p => p.ContentRootPath).Returns(Path.GetDirectoryName(menuFullPath));
        webHost.Setup(w => w.EnvironmentName).Returns("Testing");

        var gitLab = new Mock<IGitLabService>();
        gitLab
            .Setup(g => g.Push(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .Verifiable();

        if (!ctx.Set<User>().Any(u => u.Id == "1"))
        {
            ctx.Set<User>().Add(new User
            {
                Id = "1",
                Email = "json-git@mock.com",
            });
            ctx.SaveChanges();
        }

        var httpContextAccessor = ServicesFactory.GetHttpContextAccessor(new List<Claim>
            {
                new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1"),
            });

        return (
            webHost.Object,
            gitLab,
            httpContextAccessor,
            menuFullPath);
    }

    private (JsonGitFooterMenuDataProvider, Mock<IGitLabService>, string) GetJsonGitFooterMenuService(IDbContext ctx)
    {
        if (ctx is not DataContext _ctx)
            throw new ArgumentException(nameof(ctx));

        var (webHost, gitLab, httpContextAccessor, menuFullPath) = GetJsonGitMenuParams(_ctx);

        var options = new Mock<IOptionsSnapshot<FooterMenuSettings>>();
        options.Setup(p => p.Value).Returns(new FooterMenuSettings() { Path = Path.GetFileName(menuFullPath) });

        return (
            new JsonGitFooterMenuDataProvider(
                webHost,
                gitLab.Object,
                httpContextAccessor,
                Core.Membership.Test.ServicesFactory.GetUserManager(_ctx),
                options.Object),
            gitLab,
            menuFullPath);
    }

    private (JsonGitMenuDataProvider, Mock<IGitLabService>, string) GetJsonGitMenuService(IDbContext ctx)
    {
        if (ctx is not DataContext _ctx)
            throw new ArgumentException(nameof(ctx));

        var (webHost, gitLab, httpContextAccessor, menuFullPath) = GetJsonGitMenuParams(_ctx);

        var options = new Mock<IOptionsSnapshot<MenuSettings>>();
        options.Setup(p => p.Value).Returns(new MenuSettings() { Path = Path.GetFileName(menuFullPath) });

        return (
            new JsonGitMenuDataProvider(
                webHost,
                gitLab.Object,
                httpContextAccessor,
                Core.Membership.Test.ServicesFactory.GetUserManager(_ctx as DataContext),
                options.Object),
            gitLab,
            menuFullPath);
    }

    private IFooterMenuService GetService(IDbContext ctx, int innerProviderType)
    {
        if (ctx is not DataContext _ctx)
            throw new ArgumentException(nameof(ctx));

        IFooterMenuDataProvider innerService = null;
        switch (innerProviderType)
        {
            case (int)MenuProviderType.DbMenuProvider:
                innerService = new DbFooterMenuDataProvider(_ctx, this.mapper);
                break;
            case (int)MenuProviderType.JsonMenuProvider:
                var jsonFullPath = Path.GetTempFileName();
                var mockHosting = new Mock<IWebHostEnvironment>();
                mockHosting.Setup(m => m.ContentRootPath).Returns(Path.GetDirectoryName(jsonFullPath));

                var mockGitlabService = new Mock<IGitLabService>();
                var mockOptions = new Mock<IOptionsSnapshot<FooterMenuSettings>>();
                mockOptions.Setup(m => m.Value).Returns(new FooterMenuSettings { Path = Path.GetFileName(jsonFullPath) });

                innerService = new JsonGitFooterMenuDataProvider(
                    mockHosting.Object,
                    mockGitlabService.Object,
                    ServicesFactory.GetHttpContextAccessor(),
                    Core.Membership.Test.ServicesFactory.GetUserManager(_ctx),
                    mockOptions.Object);
                break;
            default:
                throw new ArgumentException("Unknown inner service type");
        }
        return new FooterMenuService(innerService);
    }

    private static FooterMenuItemDTO GetEntity()
    {
        var faker = new Faker<FooterMenuItemDTO>().
            RuleFor(s => s.Name, f => f.Random.AlphaNumeric(20)).
            RuleFor(s => s.RouterLink, f => f.Random.AlphaNumeric(10));
        return faker.Generate();
    }

    [Theory]
    [InlineData((int)MenuProviderType.DbMenuProvider)]
    //TODO: fix for JsonMenuProvider
    //[InlineData((int)MenuProviderType.JsonMenuProvider)]
    public async Task SaveExistsTest(int serviceType)
    {
        // Arrange
        var ctx = SutDataHelper.CreateEmptyContext();
        var entity = GetEntity();
        var service = this.GetService(ctx, serviceType);
        var stored = await service.Save(entity, CancellationToken.None);

        // Act
        var result1 = await service.Exists(stored.Id);
        var result2 = await service.Exists(stored.Id + 1);

        // Assert
        Assert.True(result1);
        Assert.False(result2);
    }

    [Theory]
    [InlineData((int)MenuProviderType.DbMenuProvider)]
    //TODO: fix for JsonMenuProvider
    //[InlineData((int)MenuProviderType.JsonMenuProvider)]
    public async Task GetAllAsyncTest(int serviceType)
    {
        // Arrange
        var ctx = SutDataHelper.CreateEmptyContext();
        var service = this.GetService(ctx, serviceType);
        var entites = new List<FooterMenuItemDTO>
            {
                GetEntity(),
                GetEntity(),
                GetEntity(),
            };
        entites.ForEach(async e => await service.Save(e, CancellationToken.None));

        // Act
        var result = await service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(entites.Count, result.Count);
        Assert.Contains(result, r => entites.FirstOrDefault().Name.Equals(r.Name));
    }

    [Theory]
    [InlineData((int)MenuProviderType.DbMenuProvider)]
    //TODO: fix for JsonMenuProvider
    //[InlineData((int)MenuProviderType.JsonMenuProvider)]
    public async Task DeleteTest(int serviceType)
    {
        // Arrange
        var ctx = SutDataHelper.CreateEmptyContext();
        var entity = GetEntity();
        var service = this.GetService(ctx, serviceType);
        var stored = await service.Save(entity, CancellationToken.None);

        // Act
        await service.Delete(stored.Id, CancellationToken.None);
        var result = (await service.GetAllAsync()).Any(r => r.Name.Equals(entity.Name));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Add_range_Test()
    {
        var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var service = GetDbFooterService(ctx);

        var enteties = new Faker<FooterMenuItemDTO>();
        enteties.RuleFor(p => p.Id, s => s.Random.Int());
        enteties.RuleFor(p => p.Name, s => s.Random.AlphaNumeric(7));
        enteties.RuleFor(p => p.OrderNo, s => s.Random.Int());
        enteties.RuleFor(p => p.RouterLink, s => s.Random.AlphaNumeric(7));

        var result = await service.AddRange(enteties.Generate(5), CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Add_range_Test2()
    {
        // Arrange
        var ctx = SutDataHelper.CreateEmptyContext();
        var (service, gitLabService, menuFullPath) = GetJsonGitFooterMenuService(ctx);
        var (service2, gitLabService2, menuFullPath2) = GetJsonGitMenuService(ctx);

        var entitiesId = 1;
        var entities = new Faker<FooterMenuItemDTO>()
            .RuleFor(p => p.Id, s => entitiesId++)
            .RuleFor(p => p.Name, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.OrderNo, s => s.Random.Int())
            .RuleFor(p => p.RouterLink, s => s.Random.AlphaNumeric(7));

        var entities2Id = 1;
        var entities2 = new Faker<MenuDTO>();
        entities2.RuleFor(p => p.Id, s => entities2Id++)
            .RuleFor(p => p.Hidden, s => s.Random.Bool())
            .RuleFor(p => p.Href, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.RouterLink, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.ParentId, s => entities2Id > 4 ? s.PickRandom(1, 2, 3) : null);

        var saveEntites = entities.Generate(5);

        // Act
        var result = await service.AddRange(saveEntites, CancellationToken.None);
        var testGetter = await service.Get(saveEntites[0].Id, CancellationToken.None);
        var testSave = await service.Save(entities.Generate(), CancellationToken.None);

        await service2.AddRange(entities2.Generate(10), CancellationToken.None);
        await service2.AddRange(entities2.Generate(5), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(testGetter);
        Assert.NotNull(testSave);

        var menuFileInfo = new FileInfo(menuFullPath);
        Assert.True(menuFileInfo.Exists && menuFileInfo.Length > 0);
        gitLabService.Verify();

        var menuFileInfo2 = new FileInfo(menuFullPath2);
        Assert.True(menuFileInfo2.Exists && menuFileInfo2.Length > 0);
        gitLabService2.Verify();
    }

    [Theory]
    [InlineData((int)MenuProviderType.DbMenuProvider)]
    //TODO: fix for JsonMenuProvider
    //[InlineData((int)MenuProviderType.JsonMenuProvider)]
    public async Task UpdateOrderOfItemsTest(int serviceType)
    {
        // Arrange
        var e1Name = "Test 1";
        var e2Name = "Test 2";
        var entity1 = new FooterMenuItemDTO
        {
            Name = e1Name,
            OrderNo = 1,
            RouterLink = "route 1",
        };
        var entity2 = new FooterMenuItemDTO
        {
            Name = e2Name,
            OrderNo = 2,
            RouterLink = "route 2",
        };
        var ctx = SutDataHelper.CreateEmptyContext();
        var entites = new List<FooterMenuItemDTO> { entity1, entity2 };
        var service = this.GetService(ctx, serviceType);
        entites.ForEach(async e => await service.Save(e, CancellationToken.None));
        var stored = await service.GetAllAsync(CancellationToken.None);
        stored.Single(s => s.Name.Equals(e1Name)).OrderNo = 2;
        stored.Single(s => s.Name.Equals(e2Name)).OrderNo = 1;

        // Act
        await service.UpdateOrderOfItems(stored, CancellationToken.None);
        var result = await service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Single(r => r.Name.Equals(e1Name)).OrderNo);
        Assert.Equal(1, result.Single(r => r.Name.Equals(e2Name)).OrderNo);
    }
}
