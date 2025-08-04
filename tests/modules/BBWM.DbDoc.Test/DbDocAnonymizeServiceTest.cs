using Xunit;

namespace BBWM.DbDoc.Tests;

public class DbDocAnonymizeServiceTest
{
    [Fact]
    public async Task ColumnMetadataExists_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        var dbDocAnonymizeService = servicesFactory.DbDocAnonymizeService;
        await dbDocSyncService.Synchronize();


        var firstTable = (await dbDocService.GetDefaultFolder()).Tables.First();
        firstTable.Anonymization = Enums.AnonymizationAction.Anonymize;
        await dbDocService.UpdateTableMetadata(firstTable.Id, firstTable);

        var firstColumn = firstTable.Columns.First();
        firstColumn.AnonymizationRule = Enums.AnonymizationRule.RandomCharacters;
        await dbDocService.UpdateColumnMetadata(firstColumn.Id, firstColumn);

        Assert.NotEmpty(await dbDocAnonymizeService.GetAnonymizationXml(default));

        await servicesFactory.DeleteServices();
    }
}
