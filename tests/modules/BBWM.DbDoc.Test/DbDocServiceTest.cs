using System.Text.Json;
using System.Text.Json.Serialization;
using BBWM.Core.Exceptions;
using BBWM.Core.Utils;
using BBWM.DbDoc.Core.Classes;
using BBWM.DbDoc.Core.Classes.ValidationRules;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Model;
using BBWM.DbDoc.Services;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace BBWM.DbDoc.Tests;

public class DbDocServiceTest
{
    public const string TmpJsonFilePath = "dbdoc-tmp.json";

    private readonly JsonSerializerOptions _jsonSerializerOptions;


    public DbDocServiceTest()
    {
        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerOptionsProvider.OptionsWithoutCustomConverters);
        _jsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    }


    [Fact]
    public async Task ColumnMetadataExists_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        Assert.False(await dbDocService.ColumnMetadataExists(0));
        Assert.True(await dbDocService.ColumnMetadataExists((await context.Set<ColumnMetadata>().FirstAsync()).Id));

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task CopyTableMetadataToFolder_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        var defaultFolder = await dbDocService.GetDefaultFolder();
        var addTableToFolderDto = new CopyTableMetadataToFolderDTO
        {
            FolderIdCopyTo = Guid.Empty,
            CopyingTableMetadataId = 0
        };

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.CopyTableMetadataToFolder(addTableToFolderDto));

        addTableToFolderDto.FolderIdCopyTo = defaultFolder.Id;

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.CopyTableMetadataToFolder(addTableToFolderDto));

        addTableToFolderDto.CopyingTableMetadataId = defaultFolder.Tables.First().Id;

        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocService.CopyTableMetadataToFolder(addTableToFolderDto));

        var newFolder = new FolderDTO { Name = "New Folder" };
        newFolder = await dbDocService.CreateFolder(newFolder);

        addTableToFolderDto.FolderIdCopyTo = newFolder.Id;

        await dbDocService.CopyTableMetadataToFolder(addTableToFolderDto);
        newFolder = await dbDocService.GetFolder(newFolder.Id);

        Assert.NotEmpty(newFolder.Tables);
        Assert.Equal(defaultFolder.Tables.First().TableId, newFolder.Tables.First().TableId);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task CreateFolder_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        var newFolder = new FolderDTO { Name = string.Empty };

        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocService.CreateFolder(newFolder));

        newFolder.Name = DbDocService.DefaultFolderName;

        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocService.CreateFolder(newFolder));

        var newFolderName = "New Folder";
        newFolder.Name = newFolderName;
        await dbDocService.CreateFolder(newFolder);
        var allStructure = await dbDocService.GetAllStructure();

        Assert.Contains(allStructure, folder => folder.Name == newFolderName);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task DeleteColumnValidationMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.DeleteColumnValidationMetadata(0));

        var column = (await dbDocService.GetDefaultFolder()).Tables.First().Columns.First();
        var columnValidationMetadata = await dbDocService.SetValidationMetadata(column.Id, new ColumnValidationMetadataDTO
        {
            Rules = new ValidationRule[] { new RequiredValidationRule() }
        });
        await dbDocService.DeleteColumnValidationMetadata(column.Id);

        Assert.True(context.Set<ColumnValidationMetadata>().All(x => x.Id != columnValidationMetadata.Id));

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task DeleteColumnViewMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.DeleteColumnViewMetadata(0));

        var column = (await dbDocService.GetDefaultFolder()).Tables.First().Columns.First();
        var columnViewMetadata = await dbDocService.SetViewMetadata(column.Id, new ColumnViewMetadataDTO
        {
            GridColumnView = new GridColumnViewDTO
            {
                Mask = "__-__"
            }
        });
        await dbDocService.DeleteColumnViewMetadata(column.Id);

        Assert.True(context.Set<ColumnViewMetadata>().All(x => x.Id != columnViewMetadata.Id));

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task DeleteFolder_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.DeleteFolder(Guid.Empty));

        var defaultFolder = await dbDocService.GetDefaultFolder();
        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocService.DeleteFolder(defaultFolder.Id));

        var newFolder = await dbDocService.CreateFolder(new FolderDTO { Name = "New Folder" });
        var table = await dbDocService.CopyTableMetadataToFolder(
            new CopyTableMetadataToFolderDTO
            {
                FolderIdCopyTo = newFolder.Id,
                CopyingTableMetadataId = defaultFolder.Tables.First().Id
            });
        var fistColumnId = table.Columns.First().Id;
        var validationMetadata = await dbDocService.SetValidationMetadata(fistColumnId, new ColumnValidationMetadataDTO
        {
            Rules = new ValidationRule[] { new RequiredValidationRule() }
        });
        var viewMetadata = await dbDocService.SetViewMetadata(fistColumnId, new ColumnViewMetadataDTO
        {
            GridColumnView = new GridColumnViewDTO
            {
                Mask = "__-__"
            }
        });
        newFolder = await dbDocService.GetFolder(newFolder.Id);
        await dbDocService.DeleteFolder(newFolder.Id);

        Assert.True(context.Set<GridColumnView>().All(x => x.Id != viewMetadata.GridColumnView.Id));
        Assert.True(context.Set<ColumnViewMetadata>().All(x => x.Id != viewMetadata.Id));
        Assert.True(context.Set<ColumnValidationMetadata>().All(x => x.Id != validationMetadata.Id));
        Assert.True(context.Set<ColumnMetadata>().All(x => x.Id != fistColumnId));
        Assert.True(context.Set<TableMetadata>().All(x => x.Id != table.Id));
        Assert.True(context.Set<Folder>().All(x => x.Id != newFolder.Id));

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task DeleteTableMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.DeleteTableMetadata(0));

        var defaultFolder = await dbDocService.GetDefaultFolder();

        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocService.DeleteTableMetadata(defaultFolder.Tables.First().Id));

        var newFolder = await dbDocService.CreateFolder(new FolderDTO { Name = "New Folder" });
        var table = await dbDocService.CopyTableMetadataToFolder(
            new CopyTableMetadataToFolderDTO
            {
                FolderIdCopyTo = newFolder.Id,
                CopyingTableMetadataId = defaultFolder.Tables.First().Id
            });
        var fistColumnId = table.Columns.First().Id;
        var validationMetadata = await dbDocService.SetValidationMetadata(fistColumnId, new ColumnValidationMetadataDTO
        {
            Rules = new ValidationRule[] { new RequiredValidationRule() }
        });
        var viewMetadata = await dbDocService.SetViewMetadata(fistColumnId, new ColumnViewMetadataDTO
        {
            GridColumnView = new GridColumnViewDTO
            {
                Mask = "__-__"
            }
        });
        table = await dbDocService.GetTableMetadata(table.Id);
        await dbDocService.DeleteTableMetadata(table.Id);

        Assert.True(context.Set<GridColumnView>().All(x => x.Id != viewMetadata.GridColumnView.Id));
        Assert.True(context.Set<ColumnViewMetadata>().All(x => x.Id != viewMetadata.Id));
        Assert.True(context.Set<ColumnValidationMetadata>().All(x => x.Id != validationMetadata.Id));
        Assert.True(context.Set<ColumnMetadata>().All(x => x.Id != fistColumnId));
        Assert.True(context.Set<TableMetadata>().All(x => x.Id != table.Id));

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task FindTableMetadataInFolder_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();


        Assert.Null(await dbDocService.FindTableMetadataInFolder("NotExistingTableId"));

        var table = await context.Set<TableMetadata>().FirstAsync();

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.FindTableMetadataInFolder(table.TableId, Guid.Empty));

        var foundTable = await dbDocService.FindTableMetadataInFolder(table.TableId);
        Assert.NotNull(foundTable);
        Assert.Equal(table.Id, foundTable.Id);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task FindColumnMetadataInFolder_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        Assert.Null(await dbDocService.FindColumnMetadataInFolder("NotExistingColumnId"));

        var column = (await context.Set<TableMetadata>().FirstAsync()).Columns.First();

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.FindColumnMetadataInFolder(column.ColumnId, Guid.Empty));

        var foundColumn = await dbDocService.FindColumnMetadataInFolder(column.ColumnId);
        Assert.NotNull(foundColumn);
        Assert.Equal(column.Id, foundColumn.Id);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task FolderExists_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        Assert.True(await dbDocService.FolderExists((await dbDocService.GetDefaultFolder()).Id));
        Assert.False(await dbDocService.FolderExists(Guid.Empty));

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task GetAllStructure_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        var allStructureFolders = (await dbDocService.GetAllStructure()).ToList();
        var allStructureTables = allStructureFolders.SelectMany(x => x.Tables).ToList();
        var allStructureColumns = allStructureTables.SelectMany(x => x.Columns).ToList();
        var allFoldersInDb = context.Set<Folder>().ToList();
        var allTablesInDb = context.Set<TableMetadata>().ToList();
        var allColumnsInDb = context.Set<ColumnMetadata>().ToList();

        foreach (var dbDocFolder in allFoldersInDb)
            Assert.Contains(allStructureFolders, folder => folder.Id == dbDocFolder.Id);

        foreach (var tableMetadata in allTablesInDb)
            Assert.Contains(allStructureTables, table => table.Id == tableMetadata.Id);

        foreach (var columnMetadata in allStructureColumns)
            Assert.Contains(allColumnsInDb, table => table.Id == columnMetadata.Id);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task GetColumnMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        var columnFromDb = context.Set<TableMetadata>().First().Columns.First();

        Assert.Null(await dbDocService.GetColumnMetadata(0));
        Assert.NotNull(await dbDocService.GetColumnMetadata(columnFromDb.Id));

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task GetFolder_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        var folderFromDb = context.Set<Folder>().First();

        Assert.Null(await dbDocService.GetFolder(Guid.Empty));
        Assert.NotNull(await dbDocService.GetFolder(folderFromDb.Id));

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task GetDefaultFolder_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        var defaultFolder = await dbDocService.GetDefaultFolder();

        Assert.NotNull(defaultFolder);
        Assert.Equal(DbDocService.DefaultFolderName, defaultFolder.Name);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task GetTableMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        var tableFromDb = context.Set<TableMetadata>().First();

        Assert.Null(await dbDocService.GetTableMetadata(0));
        Assert.NotNull(await dbDocService.GetTableMetadata(tableFromDb.Id));

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task GetActualTableMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        var defaultFolder = await dbDocService.GetDefaultFolder();
        var newFolder = await dbDocService.CreateFolder(new FolderDTO { Name = "New Folder Name" });
        var copiedTable = await dbDocService.CopyTableMetadataToFolder(
            new CopyTableMetadataToFolderDTO
            {
                FolderIdCopyTo = newFolder.Id,
                CopyingTableMetadataId = defaultFolder.Tables.First().Id
            });

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.GetActualTableMetadata(copiedTable.TableId, Guid.Empty));
        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocService.GetActualTableMetadata("NotExistingTableId", newFolder.Id));

        var firstTableFirstNonKeyColumn = copiedTable.Columns.First(x =>
            (x.StaticData.IsPrimaryKey == null || !(bool)x.StaticData.IsPrimaryKey) && (x.StaticData.IsForeignKey == null || !(bool)x.StaticData.IsForeignKey));
        await dbDocService.SetValidationMetadata(
            firstTableFirstNonKeyColumn.Id,
            new ColumnValidationMetadataDTO { Rules = new ValidationRule[] { new MaxLengthValidationRule() } });

        var tableMetadata = await dbDocService.GetActualTableMetadata(copiedTable.TableId, newFolder.Id);

        Assert.Contains(tableMetadata, x => x.Key == firstTableFirstNonKeyColumn.StaticData.ColumnName);

        var columnMetadata = tableMetadata[firstTableFirstNonKeyColumn.StaticData.ColumnName];

        Assert.NotNull(columnMetadata.ValidationRules);
        Assert.NotEmpty(columnMetadata.ValidationRules);
        Assert.IsType<MaxLengthValidationRule>(columnMetadata.ValidationRules.First());

        var columnType = new ColumnType
        {
            Name = "Column Type Name",
            ValidationMetadata = new ColumnValidationMetadata
            {
                Rules = JsonSerializer.Serialize(new ValidationRule[]
                {
                    new RequiredValidationRule()
                }, _jsonSerializerOptions)
            }
        };
        await context.Set<ColumnType>().AddAsync(columnType);
        await context.SaveChangesAsync();

        firstTableFirstNonKeyColumn.ColumnTypeId = columnType.Id;
        await dbDocService.UpdateColumnMetadata(firstTableFirstNonKeyColumn.Id, firstTableFirstNonKeyColumn);

        tableMetadata = await dbDocService.GetActualTableMetadata(copiedTable.TableId, newFolder.Id);

        Assert.Contains(tableMetadata, x => x.Key == firstTableFirstNonKeyColumn.StaticData.ColumnName);

        columnMetadata = tableMetadata[firstTableFirstNonKeyColumn.StaticData.ColumnName];

        Assert.NotNull(columnMetadata.ValidationRules);
        Assert.NotEmpty(columnMetadata.ValidationRules);
        Assert.IsType<RequiredValidationRule>(columnMetadata.ValidationRules.First());

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task SetColumnTypeMetadataForColumnMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocService = servicesFactory.DbDocService;
        var columnTypeService = servicesFactory.ColumnTypeService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        var defaultFolderFirstTableFirstColumn = (await dbDocService.GetDefaultFolder()).Tables.First().Columns.First();

        defaultFolderFirstTableFirstColumn.AnonymizationRule = Enums.AnonymizationRule.Date;
        defaultFolderFirstTableFirstColumn = await dbDocService.UpdateColumnMetadata(defaultFolderFirstTableFirstColumn.Id, defaultFolderFirstTableFirstColumn);

        await dbDocService.SetValidationMetadata(defaultFolderFirstTableFirstColumn.Id,
            new ColumnValidationMetadataDTO
            {
                Rules = new ValidationRule[] { new RequiredValidationRule() }
            });

        var gridColumnViewFirstMask = "__-__";
        await dbDocService.SetViewMetadata(defaultFolderFirstTableFirstColumn.Id,
            new ColumnViewMetadataDTO
            {
                GridColumnView = new GridColumnViewDTO
                {
                    Mask = gridColumnViewFirstMask
                }
            });

        var gridColumnViewSecondMask = "_-_";
        var columnType = await columnTypeService.Create(new ColumnTypeDTO
        {
            Name = "Test",
            AnonymizationRule = Enums.AnonymizationRule.ElvenName,
            ValidationMetadata = new ColumnValidationMetadataDTO
            {
                Rules = new ValidationRule[] { new InputFormatValidationRule() }
            },
            ViewMetadata = new ColumnViewMetadataDTO
            {
                GridColumnView = new GridColumnViewDTO
                {
                    Mask = gridColumnViewSecondMask
                }
            }
        });

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.SetColumnTypeMetadataForColumnMetadata(0, columnType.Id));
        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.SetColumnTypeMetadataForColumnMetadata(defaultFolderFirstTableFirstColumn.Id, Guid.Empty));

        await dbDocService.SetColumnTypeMetadataForColumnMetadata(defaultFolderFirstTableFirstColumn.Id, columnType.Id);
        defaultFolderFirstTableFirstColumn = await dbDocService.GetColumnMetadata(defaultFolderFirstTableFirstColumn.Id);

        Assert.Equal(Enums.AnonymizationRule.ElvenName, defaultFolderFirstTableFirstColumn.AnonymizationRule);
        Assert.NotNull(defaultFolderFirstTableFirstColumn.ValidationMetadata);
        Assert.NotEmpty(defaultFolderFirstTableFirstColumn.ValidationMetadata.Rules);
        Assert.True(defaultFolderFirstTableFirstColumn.ValidationMetadata.Rules.First() is InputFormatValidationRule);
        Assert.NotNull(defaultFolderFirstTableFirstColumn.ViewMetadata);
        Assert.NotNull(defaultFolderFirstTableFirstColumn.ViewMetadata.GridColumnView);
        Assert.Equal(gridColumnViewSecondMask, defaultFolderFirstTableFirstColumn.ViewMetadata.GridColumnView.Mask);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task SetValidationMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.SetValidationMetadata(0, null));

        var defaultFolder = await dbDocService.GetDefaultFolder();
        await dbDocService.SetValidationMetadata(defaultFolder.Tables.First().Columns.First().Id,
            new ColumnValidationMetadataDTO
            {
                Rules = new ValidationRule[] { new RequiredValidationRule() }
            });
        var firstColumnFirstTable = await dbDocService.GetColumnMetadata(defaultFolder.Tables.First().Columns.First().Id);

        Assert.NotNull(firstColumnFirstTable.ValidationMetadata);
        Assert.NotEmpty(firstColumnFirstTable.ValidationMetadata.Rules);
        Assert.True(firstColumnFirstTable.ValidationMetadata.Rules.First() is RequiredValidationRule);

        await dbDocService.SetValidationMetadata(defaultFolder.Tables.First().Columns.First().Id,
            new ColumnValidationMetadataDTO
            {
                Rules = new ValidationRule[] { new InputFormatValidationRule() }
            });
        firstColumnFirstTable = await dbDocService.GetColumnMetadata(defaultFolder.Tables.First().Columns.First().Id);

        Assert.True(firstColumnFirstTable.ValidationMetadata.Rules.First() is InputFormatValidationRule);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task SetViewMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.SetViewMetadata(0, null));

        var defaultFolder = await dbDocService.GetDefaultFolder();
        var gridColumnViewFirstMask = "__-__";
        await dbDocService.SetViewMetadata(defaultFolder.Tables.First().Columns.First().Id,
            new ColumnViewMetadataDTO
            {
                GridColumnView = new GridColumnViewDTO
                {
                    Mask = gridColumnViewFirstMask
                }
            });
        var firstColumnFirstTable = await dbDocService.GetColumnMetadata(defaultFolder.Tables.First().Columns.First().Id);

        Assert.NotNull(firstColumnFirstTable.ViewMetadata);
        Assert.NotNull(firstColumnFirstTable.ViewMetadata.GridColumnView);
        Assert.Equal(gridColumnViewFirstMask, firstColumnFirstTable.ViewMetadata.GridColumnView.Mask);

        var gridColumnViewSecondMask = "__-__-__";
        await dbDocService.SetViewMetadata(defaultFolder.Tables.First().Columns.First().Id,
            new ColumnViewMetadataDTO
            {
                GridColumnView = new GridColumnViewDTO
                {
                    Mask = gridColumnViewSecondMask
                }
            });
        firstColumnFirstTable = await dbDocService.GetColumnMetadata(defaultFolder.Tables.First().Columns.First().Id);

        Assert.Equal(gridColumnViewSecondMask, firstColumnFirstTable.ViewMetadata.GridColumnView.Mask);

        await dbDocService.SetViewMetadata(defaultFolder.Tables.First().Columns.First().Id,
            new ColumnViewMetadataDTO());
        firstColumnFirstTable = await dbDocService.GetColumnMetadata(defaultFolder.Tables.First().Columns.First().Id);

        Assert.Null(firstColumnFirstTable.ViewMetadata.GridColumnView);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task GetValidationRulesForModel_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        await Assert.ThrowsAsync<ObjectNotExistsException>(async () =>
            await dbDocService.GetValidationRulesForModel(typeof(ColumnType), Guid.Empty));

        var newFolder = new FolderDTO { Name = "New Folder" };
        newFolder = await dbDocService.CreateFolder(newFolder);

        await Assert.ThrowsAsync<BusinessException>(async () =>
            await dbDocService.GetValidationRulesForModel(typeof(ColumnType), newFolder.Id));

        var defaultFolderColumnTypeTableMetadata = (await dbDocService.GetDefaultFolder())
            .Tables.Single(x => x.StaticData.ClrType == typeof(ColumnType).ToString());

        var addTableToFolderDto = new CopyTableMetadataToFolderDTO
        {
            FolderIdCopyTo = newFolder.Id,
            CopyingTableMetadataId = defaultFolderColumnTypeTableMetadata.Id
        };
        await dbDocService.CopyTableMetadataToFolder(addTableToFolderDto);

        var newFolderColumnTypeTableMetadata = (await dbDocService.GetFolder(newFolder.Id)).
            Tables.Single(x => x.StaticData.ClrType == typeof(ColumnType).ToString());

        await dbDocService.SetValidationMetadata(
            defaultFolderColumnTypeTableMetadata.Columns.Single(x => x.StaticData.ColumnName == nameof(ColumnType.Name)).Id,
            new ColumnValidationMetadataDTO { Rules = new[] { new RequiredValidationRule() } });

        await dbDocService.SetValidationMetadata(
            newFolderColumnTypeTableMetadata.Columns.Single(x => x.StaticData.ColumnName == nameof(ColumnType.Name)).Id,
            new ColumnValidationMetadataDTO { Rules = new[] { new RequiredValidationRule() } });

        var defaultFolderColumnTypeValidationRules = await dbDocService.GetValidationRulesForModel(typeof(ColumnType), CancellationToken.None);
        var newFolderColumnTypeValidationRules = await dbDocService.GetValidationRulesForModel(typeof(ColumnType), newFolder.Id);

        Assert.NotEmpty(defaultFolderColumnTypeValidationRules.Keys);
        Assert.NotEmpty(newFolderColumnTypeValidationRules.Keys);
        Assert.True(defaultFolderColumnTypeValidationRules.Keys.First() == nameof(ColumnType.Name));
        Assert.True(newFolderColumnTypeValidationRules.Keys.First() == nameof(ColumnType.Name));

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task Synchronize_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();

        await servicesFactory.CreateDbDocServices();

        var context = servicesFactory.SqlLiteContext;
        var dbDocService = servicesFactory.DbDocService;
        var dbDocStaticDataService = servicesFactory.DbDocStaticDataService;
        var columnTypesService = servicesFactory.ColumnTypeService;

        Assert.NotEmpty(dbDocStaticDataService.TablesStaticData);
        Assert.NotEmpty(dbDocStaticDataService.ColumnsStaticData);

        CreateJsonConfigFile("{}");
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();
        DeleteJsonConfigFile();

        var allStructure = (await dbDocService.GetAllStructure()).ToList();

        Assert.Single(allStructure);

        var defaultFolder = allStructure.Single();

        Assert.Equal(defaultFolder.Name, DbDocService.DefaultFolderName);
        Assert.Equal(defaultFolder.Tables.Count, dbDocStaticDataService.TablesStaticData.Count);
        Assert.Equal(defaultFolder.Tables.SelectMany(x => x.Columns).Count(), dbDocStaticDataService.ColumnsStaticData.Count);

        var newTestFolderName = "Test Folder";
        await dbDocService.CreateFolder(new FolderDTO
        {
            Name = newTestFolderName
        });

        CreateJsonConfigFile("{}");
        await dbDocSyncService.Synchronize();
        DeleteJsonConfigFile();

        allStructure = (await dbDocService.GetAllStructure()).ToList();

        Assert.Equal(allStructure.Count, 2);
        Assert.True(allStructure.Any(x => x.Name == newTestFolderName));

        var metadataStructure = context.Set<Folder>()
            .Include(x => x.Tables)
            .ThenInclude(x => x.Columns)
            .ThenInclude(x => x.ColumnType)
            .ThenInclude(x => x.ValidationMetadata)
            .Include(x => x.Tables)
            .ThenInclude(x => x.Columns)
            .ThenInclude(x => x.ColumnType)
            .ThenInclude(x => x.ViewMetadata)
            .ThenInclude(x => x.GridColumnView)
            .Include(x => x.Tables)
            .ThenInclude(x => x.Columns)
            .ThenInclude(x => x.ValidationMetadata)
            .Include(x => x.Tables)
            .ThenInclude(x => x.Columns)
            .ThenInclude(x => x.ViewMetadata)
            .ThenInclude(x => x.GridColumnView)
            .ToList();

        await servicesFactory.DeleteServices();

        await servicesFactory.CreateDbDocServices();

        context = servicesFactory.SqlLiteContext;
        dbDocService = servicesFactory.DbDocService;
        dbDocStaticDataService = servicesFactory.DbDocStaticDataService;
        columnTypesService = servicesFactory.ColumnTypeService;
        dbDocSyncService = servicesFactory.DbDocSyncService;

        var columnType = (await context.Set<ColumnType>().AddAsync(new ColumnType
        {
            Name = "Test Column Type Name",
            ViewMetadata = new ColumnViewMetadata
            {
                GridColumnView = new GridColumnView { Mask = "Test Mask" }
            }
        })).Entity;
        await context.SaveChangesAsync();

        context.Entry(columnType).State = EntityState.Detached;
        columnType.ViewMetadata = new ColumnViewMetadata
        {
            Id = 100
        };

        var firstTableFirstColumn = metadataStructure.First(x => x.Tables.Any()).Tables.First().Columns.First();
        firstTableFirstColumn.ValidationMetadata = new ColumnValidationMetadata();

        CreateJsonConfigFile(JsonSerializer.Serialize(new DbDocJsonStructure
        {
            Folders = metadataStructure,
            ColumnTypes = new List<ColumnType>() { columnType }
        }, _jsonSerializerOptions));
        await dbDocSyncService.Synchronize();
        DeleteJsonConfigFile();

        allStructure = (await dbDocService.GetAllStructure()).ToList();

        Assert.NotNull(allStructure.First(x => x.Tables.Any()).Tables.First().Columns.First().ValidationMetadata);

        columnType = await context.Set<ColumnType>().Include(x => x.ViewMetadata).ThenInclude(x => x.GridColumnView).FirstOrDefaultAsync();

        Assert.NotNull(columnType);
        Assert.NotNull(columnType.ViewMetadata);
        Assert.Null(columnType.ViewMetadata.GridColumnView);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task TableMetadataExists_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        Assert.False(await dbDocService.TableMetadataExists(0));
        Assert.True(await dbDocService.TableMetadataExists((await dbDocService.GetDefaultFolder()).Tables.First().Id));

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task UpdateColumnMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.UpdateColumnMetadata(0, null));

        var firstColumn = (await dbDocService.GetDefaultFolder()).Tables.First().Columns.First();
        var firstColumnOldColumnId = firstColumn.ColumnId;
        var firstColumnOldTableId = firstColumn.TableId;
        var firstColumnNewDescription = "New description";

        firstColumn.ColumnId = "ChangedColumnId";

        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocService.UpdateColumnMetadata(firstColumn.Id, firstColumn));

        firstColumn.ColumnId = firstColumnOldColumnId;
        firstColumn.TableId = 0;

        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocService.UpdateColumnMetadata(firstColumn.Id, firstColumn));

        firstColumn.TableId = firstColumnOldTableId;
        firstColumn.Description = firstColumnNewDescription;

        firstColumn = await dbDocService.UpdateColumnMetadata(firstColumn.Id, firstColumn);

        Assert.Equal(firstColumnNewDescription, firstColumn.Description);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task UpdateFolder_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.UpdateFolder(Guid.Empty, null));

        var defaultFolder = await dbDocService.GetDefaultFolder();
        defaultFolder.Name += " (edited)";

        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocService.UpdateFolder(defaultFolder.Id, defaultFolder));

        var newFolder = new FolderDTO { Name = "New Folder" };
        newFolder = await dbDocService.CreateFolder(newFolder);

        newFolder.Name = string.Empty;

        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocService.UpdateFolder(newFolder.Id, newFolder));

        newFolder.Name = DbDocService.DefaultFolderName;

        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocService.UpdateFolder(newFolder.Id, newFolder));

        var newFolderName = "New Folder (Name changed)";
        newFolder.Name = newFolderName;
        await dbDocService.UpdateFolder(newFolder.Id, newFolder);
        newFolder = await dbDocService.GetFolder(newFolder.Id);

        Assert.Equal(newFolderName, newFolder.Name);

        await servicesFactory.DeleteServices();
    }

    [Fact]
    public async Task UpdateTableMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        await servicesFactory.CreateDbDocServices();
        var dbDocService = servicesFactory.DbDocService;
        var dbDocSyncService = servicesFactory.DbDocSyncService;
        await dbDocSyncService.Synchronize();

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await dbDocService.UpdateTableMetadata(0, null));

        var firstTable = (await dbDocService.GetDefaultFolder()).Tables.First();
        var firstTableOldTableId = firstTable.TableId;
        var firstTableOldFolderId = firstTable.FolderId;
        var firstTableNewDescription = "New description";

        firstTable.TableId = "ChangedTableId";

        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocService.UpdateTableMetadata(firstTable.Id, firstTable));

        firstTable.TableId = firstTableOldTableId;
        firstTable.FolderId = Guid.Empty;

        await Assert.ThrowsAnyAsync<BusinessException>(async () =>
            await dbDocService.UpdateTableMetadata(firstTable.Id, firstTable));

        firstTable.FolderId = firstTableOldFolderId;
        firstTable.Description = firstTableNewDescription;

        firstTable = await dbDocService.UpdateTableMetadata(firstTable.Id, firstTable);

        Assert.Equal(firstTableNewDescription, firstTable.Description);

        await servicesFactory.DeleteServices();
    }


    private static void CreateJsonConfigFile(string content) => File.WriteAllText(GetFilePath(), content);

    private static void DeleteJsonConfigFile() => File.Delete(GetFilePath());

    private static string GetFilePath() => Path.Combine(Directory.GetCurrentDirectory(), TmpJsonFilePath);
}
