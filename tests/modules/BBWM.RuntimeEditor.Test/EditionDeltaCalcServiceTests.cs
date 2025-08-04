using BBWM.Core.Membership.Model;
using BBWM.RuntimeEditor.services;
using Bogus;

using Xunit;

namespace BBWM.RuntimeEditor.Test;

public class EditionDeltaCalcServiceTests
{
    public EditionDeltaCalcServiceTests()
    {
    }

    private static EditionDeltaCalcService GetService() => new();

    [Fact]
    public void Get_Edition_Update_Test()
    {
        var service = GetService();

        // Assert: edition not changed
        var resultEditionNotChanged = service.GetEditionUpdate(
            GetSet1Node1EditionFake().Generate(),
            GetSet1Node1EditionFake().Generate(),
            GetSet1DictionaryFake().Generate(),
            GetFakeUser().Generate());
        Assert.Empty(resultEditionNotChanged.Edits);


        // Assert: new node edition
        var resultNewNodeEdition = service.GetEditionUpdate(
            new RteEdition(),
            GetSet1Node1EditionFake().Generate(),
            GetSet1DictionaryFake().Generate(),
            GetFakeUser().Generate());
        Assert.NotEmpty(resultNewNodeEdition.Edits);


        // Assert: updated node edition
        var updateEdition0 = GetSet1Node2EditionFake().Generate();
        var updateEdition1 = GetSet1Node2EditionFake().Generate();

        // Update existent edits values
        for (var i = 0; i < updateEdition1.Edits[0].Edits.Length; i++)
        {
            updateEdition1.Edits[0].Edits[i].Value += "aaa";
        }

        // Remove 0th edit and replace it with new edit
        updateEdition1.Edits[0].Edits[0] = new RteEdit
        {
            Attr = Set1NewEditAttr1,
            Value = new Randomizer().AlphaNumeric(7),
        };

        var resultUpdatedNodeEdition = service.GetEditionUpdate(
            updateEdition0,
            updateEdition1,
            GetSet1DictionaryFake().Generate(),
            GetFakeUser().Generate());
        Assert.NotEmpty(resultUpdatedNodeEdition.Edits);

        // Asset: update bb tooltip
        var updateTooltip0 = GetSet1Node1Tooltip1EditionFake().Generate();
        var updateTooltip1 = GetSet1Node1Tooltip2EditionFake().Generate();

        var resultUpdateTooltipNodeEdition = service.GetEditionUpdate(
            updateTooltip0,
            updateTooltip1,
            GetSet1DictionaryFake().Generate(),
            GetFakeUser().Generate());

        Assert.NotEmpty(resultUpdateTooltipNodeEdition.Edits);
    }

    private static Faker<RteEdition> GetSet1Node1EditionFake()
        => new Faker<RteEdition>()
            .RuleFor(p => p.Edits, s =>
                new RteNodeEdits[]
                {
                        new RteNodeEdits()
                        {
                            RteId = "1",
                            Edits = new RteEdit[]
                            {
                                new RteEdit() { Attr = "a1_1", Value = "v11" },
                            },
                        },
                });

    private static Faker<RteEdition> GetSet1Node2EditionFake()
        => new Faker<RteEdition>()
            .RuleFor(p => p.Edits, s =>
                new RteNodeEdits[]
                {
                        new RteNodeEdits()
                        {
                            RteId = "2",
                            Edits = new RteEdit[]
                            {
                                new RteEdit() { Attr = "a2_1", Value = "v21" },
                                new RteEdit() { Attr = "a2_2", Value = "v22" },
                                new RteEdit() { Attr = "a2_3", Value = "v23" },
                            },
                        },
                });

    private static Faker<RteEdition> GetSet1Node1Tooltip1EditionFake()
        => new Faker<RteEdition>()
            .RuleFor(p => p.Edits, s =>
                new RteNodeEdits[]
                {
                            new RteNodeEdits()
                            {
                                RteId = "1",
                                Edits = new RteEdit[]
                                {
                                    new RteEdit() { Attr = $"<{EditionDeltaCalcService.BbTooltipNode}>", Value = string.Empty },
                                    new RteEdit() { Attr = $"<{EditionDeltaCalcService.BbTooltipNode}.message>", Value = "new tooltip" },
                                    new RteEdit() { Attr = $"<{EditionDeltaCalcService.BbTooltipNode}.{EditionDeltaCalcService.DockAttr}>", Value = "right" },
                                },
                            },
                });

    private static Faker<RteEdition> GetSet1Node1Tooltip2EditionFake()
        => new Faker<RteEdition>()
            .RuleFor(p => p.Edits, s =>
                new RteNodeEdits[]
                {
                        new RteNodeEdits()
                        {
                            RteId = "1",
                            Edits = new RteEdit[]
                            {
                                new RteEdit() { Attr = $"<{EditionDeltaCalcService.BbTooltipNode}>", Value = string.Empty },
                                new RteEdit() { Attr = $"<{EditionDeltaCalcService.BbTooltipNode}.message>", Value = "new tooltip - updated" },
                                new RteEdit() { Attr = $"<{EditionDeltaCalcService.BbTooltipNode}.{EditionDeltaCalcService.DockAttr}>", Value = "left" },

                            },
                        },
                });



    private const string Set1NewEditAttr1 = "a_new_1";

    private static Faker<RteDictionary> GetSet1DictionaryFake()
        => new Faker<RteDictionary>()
            .RuleFor(p => p.Items, s => new RteDictionaryItem[]
                {
                        new RteDictionaryItem() {
                            RteId = "1",
                            Attrs = new RteDictionaryItemAttr[] {
                                new RteDictionaryItemAttr { Attr = "a1_1" },
                            },
                        },
                        new RteDictionaryItem() {
                            RteId = "2",
                            Attrs = new RteDictionaryItemAttr[] {
                                new RteDictionaryItemAttr { Attr = "a2_1" },
                                new RteDictionaryItemAttr { Attr = "a2_2" },
                                new RteDictionaryItemAttr { Attr = "a2_3" },
                                new RteDictionaryItemAttr { Attr = Set1NewEditAttr1 },
                            },
                        },
                });

    private static Faker<User> GetFakeUser()
        => new Faker<User>()
            .RuleFor(p => p.FirstName, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.LastName, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.OrganizationId, s => s.Random.Int());
}
