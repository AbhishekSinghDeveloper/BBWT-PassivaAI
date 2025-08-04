using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Test;

using BBWM.Core.Test.Fixtures;
using BBWM.Core.Test.Utils;
using BBWM.SystemSettings;

using Bogus;

using Moq;

using System.Collections.Specialized;

using Xunit;

namespace BBWM.Messages.Templates.Test;

public class EmailTemplateServiceTests : IClassFixture<MappingFixture>
{
    public IMapper Mapper { get; }

    public EmailTemplateServiceTests(MappingFixture mappingFixture)
        => Mapper = mappingFixture.DefaultMapper;

    private static EmailTemplateDTO GetEntity()
    {
        var faker = new Faker<EmailTemplateDTO>()
            .RuleFor(p => p.Id, s => 1)
            .RuleFor(p => p.Body, s => s.Random.AlphaNumeric(50))
            .RuleFor(p => p.Code, s => $"#{s.Random.Hexadecimal(6).Substring(2)}")
            .RuleFor(p => p.From, s => s.Internet.Email())
            .RuleFor(p => p.Subject, s => s.Random.AlphaNumeric(50))
            .RuleFor(p => p.Title, s => s.Random.AlphaNumeric(50));
        return faker.Generate();
    }

    private IEmailTemplateService GetService(IDbContext context)
        => new EmailTemplateService(
            context, SutDataHelper.CreateEmptyDataService(Mapper, ctx: context), Mock.Of<ISettingsService>());

    [Fact(Skip = "Implement")]
    public Task Update_equality()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task GetByCodeTest()
    {
        // Arrange
        var templateDTO = GetEntity();
        var service = GetService(SutDataHelper.CreateEmptyContext());
        await service.Create(templateDTO, CancellationToken.None);

        // Act
        var result = await service.GetByCode(templateDTO.Code, CancellationToken.None);

        // Arrange
        Assert.NotNull(result);
        Assert.Equal(templateDTO.Code, result.Code);
        Assert.Equal(templateDTO.Body, result.Body);
    }

    [Fact]
    public void BuildEmailTest()
    {
        // Arrange
        var fromResult = "fromResult";
        var subjectResult = "subjectResult";
        var bodyResult = "bodyResult";
        var template = new EmailTemplateDTO
        {
            From = "$from test",
            Subject = "$subject test",
            Body = "$body test",
        };
        var service = GetService(InMemoryDataContext.GetContext(Guid.NewGuid().ToString()));
        var tagValues = new NameValueCollection();
        tagValues.Add("$from", fromResult);
        tagValues.Add("$subject", subjectResult);
        tagValues.Add("$body", bodyResult);

        // Act
        service.BuildEmail(template, tagValues);

        // Assert
        Assert.Equal(template.From, fromResult + " test");
        Assert.Equal(template.Subject, subjectResult + " test");
        Assert.Equal(template.Body, bodyResult + " test");
    }

    [Fact]
    public async Task CheckEmailTemplateCodeTest()
    {
        // Arrange
        var t1 = GetEntity();
        var t2 = GetEntity();
        t2.Id++;
        var templates = new List<EmailTemplateDTO>
            {
                t1, t2,
            };
        var service = GetService(SutDataHelper.CreateEmptyContext());
        foreach (var t in templates)
        {
            await service.Create(t);
        }

        // Act
        var res1 = service.CheckEmailTemplateCode(t1.Code, t1.Id);
        var res2 = service.CheckEmailTemplateCode(t2.Code, t2.Id);
        var res3 = service.CheckEmailTemplateCode(t2.Code, t1.Id);

        // Assert
        Assert.True(res1);
        Assert.True(res2);
        Assert.False(res3);
    }

    [Fact]
    public void CreateBrandTest()
    {
        // Arrange
        var logoUrl = "testLogo";
        var expected = @"<div style=""width: 100%; height: 50px; background-size: cover; background-image: url('" + logoUrl + @"')'""></div>";
        var service = GetService(InMemoryDataContext.GetContext(Guid.NewGuid().ToString()));

        // Act
        var result = service.CreateBrand(logoUrl);

        // Assert
        Assert.Equal(expected, result);
    }
}
