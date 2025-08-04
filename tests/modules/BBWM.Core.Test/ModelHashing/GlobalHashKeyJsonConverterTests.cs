using AutoMapper;

using BBWM.Core.Membership.DTO;
using BBWM.Core.ModelHashing;
using BBWM.Core.Utils;

using System.Text;
using System.Text.Json;
using Xunit;

namespace BBWM.Core.Test.ModelHashing;

public class GlobalHashKeyJsonConverterTests
{
    private readonly GlobalHashKeyJsonConverterFactory _globalHashKeyJsonConverterFactory;
    private readonly IMapper _mapper;
    private readonly DataContext _context;
    private readonly IModelHashingService _modelHashingService;


    public GlobalHashKeyJsonConverterTests()
    {
        _context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
        _mapper = AutoMapperConfig.CreateMapper();
        _modelHashingService = new ModelHashingService();
        _modelHashingService.Register(_mapper, _context);
        _globalHashKeyJsonConverterFactory = new GlobalHashKeyJsonConverterFactory(_modelHashingService);
    }


    [Fact]
    public void WriteTest()
    {
        var userDto = new UserDTO
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "Mark",
            LastName = "Davis",
            Email = "some-email@box.com",
            OrganizationId = 1,
            Organization = new OrganizationDTO
            {
                Id = 1,
                Name = "Organization Name"
            },
            Groups = new List<GroupDTO>()
            {
                new GroupDTO { Id = 1, Name = "Group Name" }
            },
            Roles = new List<RoleDTO>()
            {
                new RoleDTO { Id = Guid.NewGuid().ToString(), Name = "Role Name" }
            }
        };
        var hashedOrganizationId = _modelHashingService.HashProperty(typeof(UserDTO), nameof(UserDTO.OrganizationId), (int)userDto.OrganizationId);
        var hashedGroupId = _modelHashingService.HashProperty(typeof(GroupDTO), nameof(GroupDTO.Id), userDto.Groups.ToList()[0].Id);

        var memoryStream = new MemoryStream();
        var utf8JsonWriter = new Utf8JsonWriter(memoryStream);
        var converter = _globalHashKeyJsonConverterFactory.CreateConverter(typeof(UserDTO), JsonSerializerOptionsProvider.OptionsWithoutCustomConverters) as GlobalHashKeyJsonConverterFactory.GlobalHashKeyJsonConverter<UserDTO>;
        converter.Write(utf8JsonWriter, userDto, JsonSerializerOptionsProvider.OptionsWithoutCustomConverters);
        utf8JsonWriter.Flush();
        memoryStream.Position = 0;
        var streamReader = new StreamReader(memoryStream);
        var jsonText = streamReader.ReadToEnd().Replace(" ", string.Empty);
        streamReader.Dispose();

        Assert.Contains("\"id\":\"" + userDto.Id + "\"", jsonText);
        Assert.Contains("\"organizationId\":\"" + hashedOrganizationId + "\"", jsonText);
        Assert.Contains("\"organizationId_original\":" + userDto.OrganizationId, jsonText);
        Assert.Contains("\"id\":\"" + hashedGroupId + "\"", jsonText);
        Assert.Contains("\"id_original\":" + userDto.Groups.ToList()[0].Id, jsonText);
    }

    [Fact]
    public void ReadTest()
    {
        var userId = Guid.NewGuid();
        var hashedOrganizationId = _modelHashingService.HashProperty<UserDTO>(nameof(UserDTO.OrganizationId), 1);
        var userJson = "{\"id\":\"" + userId + "\", \"firstName\":\"Mark\", \"lastName\":\"Davis\", \"organizationId\":\"" + hashedOrganizationId + "\", \"organizationId_original\":1, \"organization\":{\"id\":\"" + hashedOrganizationId + "\", \"name\":\"Organization Name\"}}";

        var converter = _globalHashKeyJsonConverterFactory.CreateConverter(typeof(UserDTO), JsonSerializerOptionsProvider.OptionsWithoutCustomConverters) as GlobalHashKeyJsonConverterFactory.GlobalHashKeyJsonConverter<UserDTO>;

        var utf8JsonReader = new Utf8JsonReader(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(userJson)));

        var deserializedUserDto = converter.Read(ref utf8JsonReader, typeof(UserDTO), JsonSerializerOptionsProvider.OptionsWithoutCustomConverters);

        Assert.NotNull(deserializedUserDto);
        Assert.NotNull(deserializedUserDto.Organization);
        Assert.Equal(userId.ToString(), deserializedUserDto.Id);
        Assert.Equal(1, deserializedUserDto.OrganizationId);
        Assert.Equal(1, deserializedUserDto.Organization.Id);
    }

    [Fact]
    public void CanConvertTest()
    {
        Assert.True(_globalHashKeyJsonConverterFactory.CanConvert(typeof(UserDTO)));
        Assert.False(_globalHashKeyJsonConverterFactory.CanConvert(typeof(LoginDTO)));
        Assert.False(_globalHashKeyJsonConverterFactory.CanConvert(typeof(string)));
        Assert.False(_globalHashKeyJsonConverterFactory.CanConvert(typeof(int)));
        Assert.False(_globalHashKeyJsonConverterFactory.CanConvert(typeof(int[])));
    }

    private class HomeController
    {
        public HomeController() { }
    }
    private static byte[] GetSalt()
    {
        var random = new System.Security.Cryptography.RNGCryptoServiceProvider();

        // Maximum length of salt
        int maxLength = 16;

        // Empty salt array
        byte[] salt = new byte[maxLength];

        // Build the random bytes
        random.GetNonZeroBytes(salt);

        // Return the string encoded salt
        return salt;
    }
}
