using BBWM.Core.Membership.Model;
using BBWM.Core.Test;
using BBWM.GitLab;
using BBWM.RuntimeEditor.interfaces;
using BBWM.RuntimeEditor.services;
using Bogus;

using Microsoft.Extensions.Options;

using Moq;

using System.Text.Json;

using Xunit;

namespace BBWM.RuntimeEditor.Test;

public class EditionStorageServiceTests
{
    private readonly DataContext context;

    public EditionStorageServiceTests()
    {
        this.context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
    }

    private IEditionStorageService GetEditionStorageService()
    {
        var optionsRuntimeSettings = new Mock<IOptionsSnapshot<RuntimeEditorSettings>>();
        optionsRuntimeSettings.Setup(p => p.Value).Returns(new RuntimeEditorSettings());

        var optionsGitlab = new Mock<IOptionsSnapshot<GitLabSettings>>();
        optionsGitlab.Setup(p => p.Value).Returns(GitLab.Test.ServicesFactory.GetGitLabSettingsFake());

        var editionStorageService = new EditionStorageService(
            optionsRuntimeSettings.Object,
            ServicesFactory.GetEditionSendManagerService(),
            new EditionDeltaCalcService(),
            Core.Test.ServicesFactory.GetWebHostEnvironment(false, Directory.GetCurrentDirectory()),
            Core.Membership.Test.ServicesFactory.GetUserManager(this.context));

        return editionStorageService;
    }

    [Fact]
    public async Task Send_Edition_Update_To_Repository_Via_Git_Api_Test()
    {
        // Assert: Save edition update
        var user = new Faker<User>().RuleFor(p => p.Email, s => s.Person.Email).Generate();
        this.context.Set<User>().Add(user);
        this.context.SaveChanges();

        ClearLocalEditionFile();
        SaveLocalDictionary();

        var result = await this.GetEditionStorageService().SaveEdition(GetEditionFake().Generate(), user.Id, default);
        Assert.NotEmpty(result.Edits);
    }

    private static RteDictionary GetDictionary()
       => new RteDictionary
       {
           Items = new RteDictionaryItem[]
            {
                    new RteDictionaryItem
                    {
                        RteId = "1",
                        Attrs = new RteDictionaryItemAttr[] { new RteDictionaryItemAttr{ Attr = "a1", Value = "v1" } },
                    },
            },
       };

    private static void SaveLocalDictionary()
    {
        File.WriteAllText(
            Path.Combine(Directory.GetCurrentDirectory(), new RuntimeEditorSettings().DictionaryFilePath.Trim('/')),
            JsonSerializer.Serialize(GetDictionary().Items));
    }

    private static void ClearLocalEditionFile()
    {
        File.WriteAllText(
            Path.Combine(Directory.GetCurrentDirectory(), new RuntimeEditorSettings().EditionFilePath.Trim('/')),
            JsonSerializer.Serialize(new RteEdition()));
    }

    private static Faker<RteEdition> GetEditionFake()
    {
        var dItem = GetDictionary().Items[0];

        return new Faker<RteEdition>()
            .RuleFor(p => p.Edits, s =>
                new RteNodeEdits[]
                {
                                new RteNodeEdits()
                                {
                                    RteId = dItem.RteId,
                                    Edits = new RteEdit[]
                                    {
                                        new RteEdit()
                                        {
                                            Attr = dItem.Attrs[0].Attr,
                                            Value = dItem.Attrs[0].Value + new Randomizer().AlphaNumeric(7),
                                        },
                                    },
                                },
                });
    }
}