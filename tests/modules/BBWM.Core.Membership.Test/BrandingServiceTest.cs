using AutoMapper;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Services;
using BBWM.Core.Test.Fixtures;
using BBWM.Core.Test.Utils;
using BBWM.FileStorage;
using BBWM.FileStorage.Test;
using BBWT.Data;
using Bogus;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using Xunit;

namespace BBWM.Core.Membership.Test;

public class BrandingServiceTest : IClassFixture<MappingFixture>, IClassFixture<FileStorageFixture>
{
    private IMapper Mapper { get; }

    private IMapper FileStorageMapper { get; }

    private Mock<IFileStorageProvider> FileStorageProvider { get; }

    public BrandingServiceTest(MappingFixture mappingFixture, FileStorageFixture storageFixture)
    {
        Mapper = mappingFixture.DefaultMapper;
        FileStorageMapper = storageFixture.Mapper;
        FileStorageProvider = storageFixture.FileStorageProvider;
    }

    [Fact]
    public async Task DeleteLogoIcon_Should_Throw_On_Missing_Branding()
    {
        // Arrange
        using IDataContext dataContext = SutDataHelper.CreateEmptyContext<IDataContext>();
        var (brandingService, _) = GetServices(dataContext);

        // Act & Assert
        await Assert.ThrowsAsync<ObjectNotExistsException>(() => brandingService.DeleteLogoIcon(1, CancellationToken.None));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteLogoIcon_Should_Delete(bool withLogoIcon)
    {
        // Arrange
        var dataService = SutDataHelper.CreateEmptyDataService<IDataContext>(Mapper);
        using var ctx = dataService.Context;

        var dtoAdd = GetEntity(withLogoIcon: withLogoIcon);

        var brandingId = (await dataService.Create<Branding, BrandingDTO>(dtoAdd, CancellationToken.None)).Id;
        var (brandingService, _) = GetServices(ctx);

        // Act
        await brandingService.DeleteLogoIcon(brandingId);

        // Assert
        var dbAddDto = await dataService.Get<Branding, BrandingDTO>(brandingId);
        Assert.NotNull(dbAddDto);
        Assert.Null(dbAddDto.LogoIcon);
        Assert.Null(dbAddDto.LogoIconId);
    }

    [Fact]
    public async Task DeleteLogoImage_Should_Throw_On_Missing_Branding()
    {
        // Arrange
        using IDataContext dataContext = SutDataHelper.CreateEmptyContext<IDataContext>();
        var (brandingService, _) = GetServices(dataContext);

        // Act & Assert
        await Assert.ThrowsAsync<ObjectNotExistsException>(() => brandingService.DeleteLogoImage(1, CancellationToken.None));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteLogoImage_Should_Delete(bool withLogoImage)
    {
        // Arrange
        var dataService = SutDataHelper.CreateEmptyDataService<IDataContext>(Mapper);
        using var ctx = dataService.Context;

        var dtoAdd = GetEntity(withLogoImage: withLogoImage);
        var brandingId = (await dataService.Create<Branding, BrandingDTO>(dtoAdd, CancellationToken.None)).Id;
        var (brandingService, _) = GetServices(ctx);

        // Act
        await brandingService.DeleteLogoImage(brandingId);

        // Assert
        var dbAddDto = await dataService.Get<Branding, BrandingDTO>(brandingId);
        Assert.Null(dbAddDto.LogoImage);
        Assert.Null(dbAddDto.LogoImageId);
    }

    [Fact]
    public async Task Must_Return_Organization_Branding()
    {
        // Arrange
        var brandingDTOwithOrganization = GetEntity();
        var brandingNew = Mapper.Map<Branding>(brandingDTOwithOrganization);
        brandingNew.Organization = new Organization();

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, Branding>(new[] { brandingNew });
        var (brandingService, _) = GetServices(ctx);

        // Act
        var branding = await brandingService.GetOrganizationBranding(brandingNew.Organization.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(branding);
        Assert.Equal(brandingNew.Organization.Id, branding.Organization.Id);
    }

    [Fact]
    public async Task File_Storage_Tests()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var dtoAdd = GetEntity();
        var brandingBefore = Mapper.Map<Branding>(dtoAdd);

        var (_, fileStorageService) = GetServices(ctx);
        var dbSet = ctx.Set<Branding>();

        // Branding After Mapping will be without Image/Logo/Organization due to MapperConfig.
        var fileDetails = Mapper.Map<FileDetails>(GetFakeFileDetailsDTO());
        fileDetails.OperationName = "TestOperationName";

        Dictionary<string, string> additionalData = new Dictionary<string, string>() { };
        additionalData.Add("user_id", "1");
        additionalData.Add("operation_name", "TestOperationName");

        await fileStorageService.UploadFiles(new FakeClass[] { new FakeClass() }, additionalData, CancellationToken.None);

        await fileStorageService.CompleteUsersFilesUploadingOperation("1", "TestOperationName", CancellationToken.None);

        await dbSet.AddAsync(brandingBefore);
        await ctx.SaveChangesAsync(CancellationToken.None);

        Assert.NotNull(dbSet);
        Assert.NotNull(fileDetails);
    }

    private (IBrandingService, IFileStorageService) GetServices(IDataContext context)
    {
        var fileStorageService = new FileStorageService(FileStorageMapper, context, FileStorageProvider.Object);
        var brandingService = new BrandingService(context, fileStorageService, new DataService(context, Mapper));

        return (brandingService, fileStorageService);
    }

    private BrandingDTO GetEntity(bool withLogoIcon = true, bool withLogoImage = true)
    {
        var faker = new Faker<BrandingDTO>()
            .RuleFor(p => p.Theme, s => s.Random.AlphaNumeric(10))
            .RuleFor(p => p.EmailBody, s => s.Random.AlphaNumeric(50))
            .RuleFor(p => p.Disabled, s => s.Random.Bool())
            .RuleFor(p => p.LogoImage, s => withLogoImage ? GetFakeFileDetailsDTO() : null)
            .RuleFor(p => p.LogoIcon, s => withLogoIcon ? GetFakeFileDetailsDTO() : null);

        return faker.Generate();
    }

    private static FileDetailsDTO GetFakeFileDetailsDTO()
    {
        var file = new Faker<FileDetailsDTO>()
            .RuleFor(p => p.Key, s => s.Random.AlphaNumeric(10))
            .RuleFor(p => p.ThumbnailKey, s => s.Random.AlphaNumeric(20))
            .RuleFor(p => p.Url, s => s.Internet.Url())
            .RuleFor(p => p.ThumbnailUrl, s => s.Internet.Url())

            .RuleFor(p => p.IsImage, s => s.Random.Bool())
            .RuleFor(p => p.FileName, s => s.System.FileName())
            .RuleFor(p => p.Size, s => s.Random.Long())

            .RuleFor(p => p.UploadTime, s => s.Date.Recent(10))
            .RuleFor(p => p.LastUpdated, s => s.Date.Recent(2));

        return file.Generate();
    }

    private class FakeClass : IFormFile
    {
        public string Id { get; set; }

        public string OperationName { get; set; }

        public string UserId { get; set; }

        public bool IsImage { get; set; }

        public string ContentType => "image/jpeg";

        public string ContentDisposition => "attachment; filename=\"key.jpg\"";

        public IHeaderDictionary Headers => new HeaderDictionary();

        public long Length => 4096;

        public string Name => throw new NotImplementedException();

        public string FileName => "key.jpeg";

        public void CopyTo(Stream target) { }

        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Stream OpenReadStream()
            => new MemoryStream(Encoding.UTF8.GetBytes("Invalid image"), false);
    }
}
