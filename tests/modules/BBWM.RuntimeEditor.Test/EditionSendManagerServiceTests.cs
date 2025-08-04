using Bogus;

using Xunit;

namespace BBWM.RuntimeEditor.Test;

public class EditionSendManagerServiceTests
{
    public EditionSendManagerServiceTests()
    {
    }

    [Fact]
    public async Task Send_Edition_Update_To_Repository_Via_Git_Api_Test()
    {
        // Assert: edition update sent via Git API. We suppose the service method shouldn't throw exception
        await ServicesFactory.GetEditionSendManagerService()
            .SendEditionUpdateToRepository(GeEditionUpdateFake().Generate(), EditionSendProviderType.GitApi, default);
    }

    [Fact]
    public async Task Send_Edition_Update_To_Repository_Via_Aws_Api_Test()
    {
        // Assert: edition update sent via AWS API. We suppose the service method shouldn't throw exception
        await ServicesFactory.GetEditionSendManagerService()
            .SendEditionUpdateToRepository(GeEditionUpdateFake().Generate(), EditionSendProviderType.AwsApi, default);
    }

    private static Faker<RteEditionUpdate> GeEditionUpdateFake()
        => new Faker<RteEditionUpdate>()
            .RuleFor(p => p.SubmittedBy, s =>
                    new RteEditionSumbmitUser { Email = s.Person.Email, Name = s.Person.FullName })
            .RuleFor(p => p.Edits, s =>
                new RteNodeEdits[]
                {
                        new RteNodeEdits
                        {
                            RteId = s.Random.AlphaNumeric(7),
                            Edits = new RteEdit[] { new RteEdit { Attr = s.Random.AlphaNumeric(7), Value = s.Random.AlphaNumeric(7) } },
                        },
                });
}
