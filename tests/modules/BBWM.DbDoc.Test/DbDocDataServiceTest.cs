using BBWM.Core.Exceptions;
using BBWM.DbDoc.Core.Classes.ValidationRules;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;

using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace BBWM.DbDoc.Tests;

public class DbDocDataServiceTest
{
    [Fact]
    public async Task DeleteTableEntity_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();
        var dbDocDataService = servicesFactory.DbDocDataService;
        

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocDataService.DeleteTableEntity("NotExistingTableId", null));

        var newColumnType = new ColumnType
        {
            Name = "TestName"
        };
        await context.Set<ColumnType>().AddAsync(newColumnType);
        await context.SaveChangesAsync();

        var columnTypeTableMetadata = (await dbDocService.GetDefaultFolder()).Tables.First(x => x.StaticData.TableName == "DbDocColumnType");

        await dbDocDataService.DeleteTableEntity(columnTypeTableMetadata.TableId, newColumnType.Id);

        Assert.DoesNotContain(context.Set<ColumnType>().ToList(), x => x.Id == newColumnType.Id);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task GetTableDump_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocService = servicesFactory.DbDocService;
        var dbDocDataService = servicesFactory.DbDocDataService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocDataService.GetTableDump("NotExistingTableId", null));

        var tableMetadataTable = (await dbDocService.GetDefaultFolder()).Tables.First(x => x.StaticData.TableName == "DbDocTableMetadata");
        var tableDump = await dbDocDataService.GetTableDump(tableMetadataTable.TableId, null);

        Assert.NotNull(tableDump);
        Assert.NotNull(tableDump.Columns);
        Assert.NotNull(tableDump.Data);
        Assert.NotEmpty(tableDump.Columns);
        Assert.NotEmpty(tableDump.Data.Items);
        Assert.NotEqual(0, tableDump.Data.Total);
        foreach (var columnName in tableMetadataTable.Columns.Select(x => x.StaticData.ColumnName))
            Assert.Contains(tableDump.Columns, tuple => tuple.Item1 == columnName);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task GetTableDumpSettings_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocDataService = servicesFactory.DbDocDataService;

        Assert.NotNull(dbDocDataService.GetDumpSettings());

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task SaveTableEntity_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocDataService = servicesFactory.DbDocDataService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocDataService.SaveTableEntity(null, 0));

        var columnTypeTableMetadata = (await dbDocService.GetDefaultFolder()).Tables.First(x => x.StaticData.TableName == "DbDocColumnType");
        var columnTypeNameColumnMetadata = columnTypeTableMetadata.Columns.First(x => x.StaticData.ColumnName == "Name");

        await dbDocService.SetValidationMetadata(columnTypeNameColumnMetadata.Id, new ColumnValidationMetadataDTO
        {
            Rules = new[] {
                    new MaxLengthValidationRule
                    {
                        MaxLength = 10
                    }
                }
        });

        var newColumnType = new ColumnType
        {
            Name = "TestName"
        };
        await context.Set<ColumnType>().AddAsync(newColumnType);
        await context.SaveChangesAsync();
        context.Entry(newColumnType).State = EntityState.Detached;

        newColumnType.Name = "0123456789 length is grater then 10";
        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocDataService.SaveTableEntity(
                JsonObject.Parse(JsonSerializer.Serialize(newColumnType)),
                columnTypeTableMetadata.Id));

        var columnTypeNewName = "NewName";
        newColumnType.Name = columnTypeNewName;
        await dbDocDataService.SaveTableEntity(
            JsonObject.Parse(JsonSerializer.Serialize(newColumnType)),
            columnTypeTableMetadata.Id);

        newColumnType = context.Set<ColumnType>().SingleOrDefault(x => x.Id == newColumnType.Id);

        Assert.Equal(columnTypeNewName, newColumnType.Name);

        await servicesFactory.DeleteServices();
    }
}