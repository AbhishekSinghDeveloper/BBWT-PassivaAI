using AutoMapper;

using BBWM.Core.Test.Fixtures;
using BBWM.Core.Test.Utils;

using Bogus;

using Xunit;

namespace BBWM.StaticPages.Test;

public class StaticPageServiceTest : IClassFixture<MappingFixture>
{
    public IMapper Mapper { get; }

    public StaticPageServiceTest(MappingFixture mappingFixture)
        => Mapper = mappingFixture.DefaultMapper;

    private static StaticPageDTO GetEntity()
    {
        var id = 0;
        var faker = new Faker<StaticPageDTO>()
            .RuleFor(p => p.Id, s => id++)
            .RuleFor(p => p.Alias, s => s.Random.AlphaNumeric(6))
            .RuleFor(p => p.Heading, s => s.Random.AlphaNumeric(6))
            .RuleFor(p => p.LastUpdated, s => s.Date.Soon())
            .RuleFor(p => p.ContentPreview, s => s.Random.AlphaNumeric(50))
            .RuleFor(p => p.Contents, s => s.Random.AlphaNumeric(50));
        return faker.Generate();
    }

    private IStaticPageService GetService(string dbName = null)
        => new StaticPageService(SutDataHelper.CreateEmptyDataService(Mapper));

    //[Fact]
    //public async Task Update_equality()
    //{
    //    //await base.Update_equality();

    //    var sut = GetService("update_equality");
    //    var dto = (await sut.GetAll()).FirstOrDefault();
    //    Assert.NotEqual(GetEntity().LastUpdated, dto.LastUpdated);
    //}

    [Fact]
    public async Task CheckExistTest()
    {
        // Arrange
        var entity1 = GetEntity();
        var entity2 = GetEntity();
        var service = GetService();
        await service.Create(entity1);

        // Act
        var result1 = await service.CheckExist(entity1);
        var result2 = await service.CheckExist(entity2);

        // Assert
        Assert.True(result1);
        Assert.False(result2);
    }

    [Fact]
    public async Task GetByUrlTest()
    {
        // Arrange
        var entity = GetEntity();
        var service = GetService();
        await service.Create(entity);

        // Act
        var result = await service.GetByUrl(entity.Alias);

        // Assert
        Assert.Equal(entity.Alias, result.Alias);
        Assert.Equal(entity.ContentPreview, result.ContentPreview);
    }
}
