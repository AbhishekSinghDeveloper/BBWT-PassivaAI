using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Test;

using Bogus;

using Microsoft.AspNetCore.Identity;

using Moq;

using Xunit;

namespace BBWM.Metadata.Test;

public class MetadataServiceTests
{
    private IDbContext _context;
    private IMapper _mapper;


    public MetadataServiceTests()
    {
    }

    public static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

        mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
        mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<TUser, string>((x, y) => ls.Add(x));
        mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);

        return mgr;
    }

    private MetadataService<MetadataModel<IdentityUser>, IdentityUser> GetService()
    {
        _context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
        _mapper = AutoMapperConfig.CreateMapper();

        var users = new Faker<IdentityUser>()
            .RuleFor(p => p.Id, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.LockoutEnabled, s => s.Random.Bool())
            .RuleFor(p => p.LockoutEnd, s => new DateTimeOffset());

        return new MetadataService<MetadataModel<IdentityUser>, IdentityUser>(_context, _mapper, ServicesFactory.GetHttpContextAccessor(), MockUserManager<IdentityUser>(users.Generate(5)).Object);
    }

    [Fact]
    public void Get_By_Key_Test()
    {
        // Arrange
        var service = GetService();

        var metadataDto = new Faker<MetadataDTO>();
        metadataDto.RuleFor(p => p.Key, s => s.Random.AlphaNumeric(7));
        metadataDto.RuleFor(p => p.Value, s => s.Random.AlphaNumeric(7));
        metadataDto.RuleFor(p => p.UserId, s => s.Random.AlphaNumeric(7));
        metadataDto.RuleFor(p => p.LockedByUserFullName, s => s.Random.AlphaNumeric(7));
        metadataDto.RuleFor(p => p.LockedByUserId, s => s.Random.AlphaNumeric(7));
        metadataDto.RuleFor(p => p.CreatedOn, s => new DateTimeOffset());
        metadataDto.RuleFor(p => p.LastUpdated, s => new DateTimeOffset());
        metadataDto.RuleFor(p => p.LastUpdated, s => new DateTimeOffset());
        metadataDto.RuleFor(p => p.IsLocked, s => s.Random.Bool());
        metadataDto.Generate();

        //var mapper = _mapper.Map<MetadataModel<IdentityUser>>(metadataDto);

        //_context.Set<MetadataModel<IdentityUser>>().Add(metadataModel.Object);
        // _context.SaveChanges();

        Action result = () => service.GetByKey("TestKey");

        Assert.NotNull(result);
        Assert.Throws<InvalidOperationException>(result);
    }

    [Fact]
    public void Save_Test1()
    {
        // Arrange
        var service = GetService();

        var metadataDto = new Faker<MetadataDTO>();
        metadataDto.RuleFor(p => p.Key, s => s.Random.AlphaNumeric(7));
        metadataDto.RuleFor(p => p.Value, s => s.Random.AlphaNumeric(7));
        metadataDto.RuleFor(p => p.UserId, s => s.Random.AlphaNumeric(7));
        metadataDto.RuleFor(p => p.LockedByUserFullName, s => s.Random.AlphaNumeric(7));
        metadataDto.RuleFor(p => p.LockedByUserId, s => s.Random.AlphaNumeric(7));
        metadataDto.RuleFor(p => p.CreatedOn, s => new DateTimeOffset());
        metadataDto.RuleFor(p => p.LastUpdated, s => new DateTimeOffset());
        metadataDto.RuleFor(p => p.LastUpdated, s => new DateTimeOffset());
        metadataDto.RuleFor(p => p.IsLocked, s => s.Random.Bool());
        metadataDto.Generate();

        Action result = () => service.Save(metadataDto);

        Assert.NotNull(result);
        Assert.Throws<AutoMapperMappingException>(result);
    }

    [Fact]
    public void Save_Test2()
    {
        // Arrange
        var service = GetService();

        Action result = () => service.Save("testKey", "testValue");

        Assert.NotNull(result);
        Assert.Throws<InvalidOperationException>(result);
    }

    [Fact]
    public void Lock_Unlock_Record_Test()
    {
        // Arrange
        var service = GetService();

        Action result = () => service.LockUnlockRecord("testKey", true);

        Assert.NotNull(result);
        Assert.Throws<InvalidOperationException>(result);
    }
}
