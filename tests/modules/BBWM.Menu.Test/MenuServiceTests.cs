using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Enums;
using BBWM.Core.Membership.Interfaces;
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
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BBWM.Menu.Test;

public class MenuServiceTests : IClassFixture<MappingFixture>
{
    private readonly IMapper mapper;

    public MenuServiceTests(MappingFixture mappingFixture)
    {
        this.mapper = mappingFixture.DefaultMapper;
    }

    private DbMenuDataProvider GetDbFooterService(IDataContext ctx)
    {
        return new DbMenuDataProvider(ctx, this.mapper);
    }

    private IMenuService GetSutService(IDbContext ctx, int innerProviderType)
    {
        IMenuDataProvider innerService = null;
        switch (innerProviderType)
        {
            case (int)MenuProviderType.DbMenuProvider:
                innerService = new DbMenuDataProvider(ctx, this.mapper);
                break;

            case (int)MenuProviderType.JsonMenuProvider:
                var jsonFullPath = Path.GetTempFileName();
                var mockHosting = new Mock<IWebHostEnvironment>();
                mockHosting.Setup(m => m.ContentRootPath).Returns(Path.GetDirectoryName(jsonFullPath));
                var mockGitlabService = new Mock<IGitLabService>();
                var mockOptions = new Mock<IOptionsSnapshot<MenuSettings>>();
                mockOptions.Setup(m => m.Value).Returns(new MenuSettings { Path = Path.GetFileName(jsonFullPath) });

                innerService = new JsonGitMenuDataProvider(
                    mockHosting.Object,
                    mockGitlabService.Object,
                    ServicesFactory.GetHttpContextAccessor(),
                    Core.Membership.Test.ServicesFactory.GetUserManager(ctx as DataContext),
                    mockOptions.Object);
                break;

            default:
                throw new ArgumentException("Unknown inner service type");
        }

        var test = new string[] { GetMenuEntity().RouterLink, GetMenuEntity().RouterLink, GetMenuEntity().RouterLink = "test" };

        var mock = new Mock<IRouteRolesService>();
        mock.Setup(p => p.GetPageRoutesForUser(It.IsAny<string>(), CancellationToken.None)).Returns(Task.FromResult(test));

        return new MenuService(innerService, this.mapper, mock.Object);
    }

    private static UserDTO GetUserEntity()
    {
        var faker = new Faker<UserDTO>()
            .RuleFor(p => p.Id, s => s.Random.Int().ToString())
            .RuleFor(p => p.FirstName, s => s.Person.FirstName)
            .RuleFor(p => p.LastName, s => s.Person.LastName)
            .RuleFor(p => p.Email, (s, p) => s.Internet.Email(p.FirstName, p.LastName))
            .RuleFor(p => p.UserName, (s, p) => p.Email)
            .RuleFor(p => p.Password, s => s.Internet.Password())
            .RuleFor(p => p.ConfirmPassword, (s, p) => p.Password)
            .RuleFor(p => p.AccountStatus, s => AccountStatus.Unapproved)
            .RuleFor(p => p.TwoFactorEnabled, p => false)
            .RuleFor(p => p.Roles, s => new List<RoleDTO>() { })
            .RuleFor(p => p.Groups, s => new List<GroupDTO>() { });

        return faker.Generate();
    }

    private static MenuDTO GetMenuEntity()
    {
        var faker = new Faker<MenuDTO>().
            RuleFor(s => s.Label, f => f.Random.AlphaNumeric(7)).
            RuleFor(s => s.RouterLink, f => f.Random.AlphaNumeric(10));
        return faker.Generate();
    }

    //private async Task DeleteTempDataAsync()
    //{
    //    // items in innerService defined as static so we need to manually clear them
    //    var service = this.GetSutService((int)MenuProviderType.JsonMenuProvider);
    //    var storedInMemory = (await service.GetAllAsync(CancellationToken.None)).Select(r => r.Id).ToList();
    //    storedInMemory.ForEach(async id => await service.Delete(id, CancellationToken.None));
    //}

    private static MenuDTO Clone(MenuDTO from) => new MenuDTO
    {
        Id = from.Id,
        Label = from.Label,
        Index = from.Index,
        RouterLink = from.RouterLink,
    };

    [Theory]
    [InlineData((int)MenuProviderType.DbMenuProvider)]
    [InlineData((int)MenuProviderType.JsonMenuProvider)]
    public async Task CreateGetAllTest(int serviceType)
    {
        // Arrange
        var ctx = SutDataHelper.CreateEmptyContext();
        var service = this.GetSutService(ctx, serviceType);
        var entity1 = GetMenuEntity();
        var entity2 = GetMenuEntity();
        var entity3 = GetMenuEntity();
        var entity4 = GetMenuEntity();
        var entity5 = GetMenuEntity();
        entity3.RouterLink = "test";
        entity3.Children = new List<MenuDTO>() { entity4, entity5 };

        var userDto = GetUserEntity();
        var entites = new List<MenuDTO> { entity1, entity2, entity3 };
        entites.ForEach(async e => await service.Create(e, CancellationToken.None));

        entity3.Children = new List<MenuDTO>() { };
        await service.Update(entity3, CancellationToken.None);

        // Act
        var result = await service.GetAllAsync(CancellationToken.None);

        var userMapper = this.mapper.Map<User>(userDto);
        ctx.Set<User>().Add(userMapper);
        ctx.SaveChanges();

        await service.GetForUser(userMapper.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(userMapper.Id);
    }

    [Theory]
    [InlineData((int)MenuProviderType.DbMenuProvider)]
    //TODO: fix for JsonMenuProvider
    //[InlineData((int)MenuProviderType.JsonMenuProvider)]
    public async Task UpdateTest(int serviceType)
    {
        // Arrange
        string firstlabel = "First";
        string secondLabel = "Second";
        string thirdLabel = "Third";
        var ctx = SutDataHelper.CreateEmptyContext();
        var service = this.GetSutService(ctx, serviceType);
        var entity = new MenuDTO
        {
            Label = "Parent",
            Children = new List<MenuDTO>
                {
                    new MenuDTO
                    {
                        Label = firstlabel,
                        Index = 0,
                    },
                    new MenuDTO
                    {
                        Label = secondLabel,
                        Index = 1,
                    },
                    new MenuDTO
                    {
                        Label = thirdLabel,
                        Index = 2,
                    },
                },
        };

        var entity2 = GetMenuEntity();
        var entity3 = GetMenuEntity();
        var entity4 = GetMenuEntity();
        var entites = new List<MenuDTO> { entity2, entity3, entity4 };
        entites.ForEach(async e => await service.Create(e, CancellationToken.None));

        var storedId = await service.Create(entity, CancellationToken.None);
        var stored = (await service.GetAllAsync(CancellationToken.None)).FirstOrDefault(s => s.Id == storedId);
        var storedSecondChild = stored.Children.FirstOrDefault(c => c.Index == 1);
        var secondChild = Clone(storedSecondChild);

        // Act
        secondChild.Index = 3;
        await service.Update(secondChild, CancellationToken.None);
        var result1 = (await service.GetAllAsync(CancellationToken.None)).FirstOrDefault(s => s.Id == storedId);
        secondChild = Clone(storedSecondChild);
        secondChild.ParentId = null;
        secondChild.Index = 1;
        await service.Update(secondChild, CancellationToken.None);

        secondChild.Index = 7;
        await service.Update(secondChild, CancellationToken.None);

        var result2 = await service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(result1.Children.ToList()[1].Index, result1.Children.Single(c => c.Label.Equals(thirdLabel)).Index);
        Assert.Equal(5, result2.Count);
        Assert.True(result2.Single(p => p.Label.Equals(secondLabel)).Index > result2.First().Index);
    }

    [Theory]
    [InlineData((int)MenuProviderType.DbMenuProvider)]
    //TODO: fix for JsonMenuProvider
    //[InlineData((int)MenuProviderType.JsonMenuProvider)]
    public async Task DeleteTest(int serviceType)
    {
        // Arrange
        var ctx = SutDataHelper.CreateEmptyContext();
        var service = this.GetSutService(ctx, serviceType);
        var entity1 = GetMenuEntity();
        var entity2 = GetMenuEntity();
        var entity3 = GetMenuEntity();

        var menuDTOEntities = new Faker<MenuDTO>();
        menuDTOEntities.RuleFor(p => p.Id, s => s.Random.Int());
        menuDTOEntities.RuleFor(p => p.Label, s => s.Random.AlphaNumeric(7));
        menuDTOEntities.RuleFor(p => p.Href, s => s.Random.AlphaNumeric(7));
        menuDTOEntities.RuleFor(p => p.RouterLink, s => s.Random.AlphaNumeric(7));

        var menuDTOEntity = new Faker<MenuDTO>();
        menuDTOEntity.RuleFor(p => p.Id, s => s.Random.Int());
        menuDTOEntity.RuleFor(p => p.Label, s => s.Random.AlphaNumeric(7));
        menuDTOEntity.RuleFor(p => p.Href, s => s.Random.AlphaNumeric(7));
        menuDTOEntity.RuleFor(p => p.RouterLink, s => s.Random.AlphaNumeric(7));
        menuDTOEntity.RuleFor(p => p.Children, s => menuDTOEntities.Generate(5));

        var entites = new List<MenuDTO> { entity1, entity2 };
        var storedId = await service.Create(entity3, CancellationToken.None);
        var storedId2 = await service.Create(menuDTOEntity, CancellationToken.None);

        // Act
        var result1 = (await service.GetAllAsync(CancellationToken.None)).Count;
        await service.Delete(storedId, CancellationToken.None);

        var result2 = (await service.GetAllAsync(CancellationToken.None)).Count;

        await service.Delete(storedId2, CancellationToken.None);

        // Assert
        Assert.Equal(result1 - 1, result2);
    }

    [Fact]
    public async Task Add_range_Test()
    {
        // Arrange
        var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var service = GetDbFooterService(ctx);

        var child = new Faker<MenuDTO>();
        child.RuleFor(p => p.Id, s => s.Random.Int());
        child.RuleFor(p => p.Label, s => s.Random.AlphaNumeric(7));
        child.RuleFor(p => p.Href, s => s.Random.AlphaNumeric(7));
        child.RuleFor(p => p.RouterLink, s => s.Random.AlphaNumeric(7));

        var entity = new Faker<MenuDTO>();
        entity.RuleFor(p => p.Id, s => s.Random.Int());
        entity.RuleFor(p => p.Label, s => s.Random.AlphaNumeric(7));
        entity.RuleFor(p => p.Href, s => s.Random.AlphaNumeric(7));
        entity.RuleFor(p => p.RouterLink, s => s.Random.AlphaNumeric(7));
        entity.RuleFor(p => p.Children, s => child.Generate(5));

        var entities = entity.Generate(5);

        // Act
        await service.AddRange(entities, CancellationToken.None);

        // Assert
        Assert.All(entities, m => Assert.NotNull(ctx.Set<MenuItem>().Find(m.Id)));
    }
}

public enum MenuProviderType
{
    DbMenuProvider,
    JsonMenuProvider,
}
