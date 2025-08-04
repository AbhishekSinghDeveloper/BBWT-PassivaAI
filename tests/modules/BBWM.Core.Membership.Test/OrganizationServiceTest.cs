using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Services;

using BBWM.Core.Test.Fixtures;
using BBWM.Core.Test.Utils;

using BBWT.Data;

using Bogus;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace BBWM.Core.Membership.Test;

public class OrganizationServiceTest : IClassFixture<MappingFixture>
{
    public IMapper Mapper { get; }

    public OrganizationServiceTest(MappingFixture mappingFixture)
        => Mapper = mappingFixture.DefaultMapper;

    [Theory]
    [InlineData(0, "")]
    [InlineData(1, "TestName")]
    public async Task Get_Test(int skipId, string name)
    {
        // Arrange
        const int ID = 2;
        const string NAME = "TestName";

        var organizationDto = GetEntity();
        organizationDto.Id = ID;
        organizationDto.Name = NAME;

        var dataService = await SutDataHelper.CreateDataServiceWithData(Mapper, new[] { Mapper.Map<Organization>(organizationDto) });
        var service = GetService(dataService.Context);

        // Act
        var dbOrganizationDto = await service.Get(name, skipId, CancellationToken.None);

        // Assert
        Assert.Equal(ID, dbOrganizationDto?.Id);
        Assert.Equal(NAME, dbOrganizationDto?.Name);
    }

    [Fact]
    public async Task DeleteOrganization()
    {
        // Arrange
        var dataService = SutDataHelper.CreateEmptyDataService(Mapper);

        var organizationDto = new OrganizationDTO
        {
            Name = "Test organization",
            Address = CreateFakeAddress(),
            Branding = new BrandingDTO { },
        };

        var id = (await dataService.Create<Organization, OrganizationDTO>(organizationDto, CancellationToken.None)).Id;

        // Act
        var organizationService = GetService(dataService.Context);
        await organizationService.Delete(id, CancellationToken.None);

        // Assert
        var dbOrganization = await dataService.Get<Organization, OrganizationDTO>(id);
        Assert.Null(dbOrganization);
    }

    [Fact]
    public async Task Delete_Should_Throw()
    {
        // Arrange
        using IDbContext dbContext = SutDataHelper.CreateEmptyContext();
        IDataService dataService = SutDataHelper.CreateEmptyDataService(Mapper, ctx: dbContext);
        OrganizationDTO organizationDTO = await dataService.Create<Organization, OrganizationDTO>(GetEntity(), CancellationToken.None);
        User user = new() { UserName = "User One", OrganizationId = organizationDTO.Id };
        await dbContext.Set<User>().AddAsync(user);
        await dbContext.SaveChangesAsync();

        IOrganizationService organizationService = GetService(dbContext);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => organizationService.Delete(organizationDTO.Id));
    }

    [Fact]
    public async Task GetEntityQuery_Should_Full_Organization()
    {
        // Arrange
        Organization organization = new()
        {
            Address = Mapper.Map<Address>(CreateFakeAddress()),
            Branding = new()
            {
                LogoIcon = new() { FileName = "blah.ico" },
                LogoImage = new() { FileName = "blah.png" },
            },
        };
        using IDataContext dataContext = await SutDataHelper.CreateContextWithData<IDataContext, Organization>(new[] { organization });
        IOrganizationService organizationService = GetService(dataContext);
        IQueryable<Organization> query = organizationService.GetEntityQuery(dataContext.Set<Organization>());

        // Act
        Organization queriedOrganization = await query.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(queriedOrganization?.Address);
        Assert.NotNull(queriedOrganization?.Branding?.LogoIcon);
        Assert.NotNull(queriedOrganization?.Branding?.LogoImageId);
    }

    private static AddressDTO CreateFakeAddress() => new AddressDTO
    {
        Address1 = "Test Address 1",
        Address2 = "Test Address 2",
        Address3 = "Test Address 3",
        Address4 = "Test Address 4",
        PostCode = "Test Post code",
    };

    private IOrganizationService GetService(IDbContext ctx)
        => new OrganizationService(new DataService(ctx, Mapper));

    private static OrganizationDTO GetEntity()
    {
        var faker = new Faker<OrganizationDTO>()
            .RuleFor(p => p.Name, s => "TestName")
            .RuleFor(p => p.Description, s => s.Random.AlphaNumeric(30))
            .RuleFor(p => p.PostCode, (s, p) => s.Address.ZipCode())
            .RuleFor(p => p.Level, s => s.Random.Int(1, 10))
            .RuleFor(p => p.Address, s => new AddressDTO() { });

        return faker.Generate();
    }
}
