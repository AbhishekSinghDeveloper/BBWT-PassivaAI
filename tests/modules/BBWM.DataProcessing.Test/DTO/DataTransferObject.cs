using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.DTO;
using Bogus;
using Moq;
using NPOI.SS.UserModel;
using System.Text.Json.Nodes;
using Xunit;

namespace BBWM.DataProcessing.Test.DTO;

public class DataTransferObject
{
    [Fact]
    public void Test_All_DTOs()
    {
        var cellDataTypeInfoDTO = new Faker<CellDataTypeInfoDTO>();
        cellDataTypeInfoDTO.RuleFor(p => p.Min, s => s.Random.Double());
        cellDataTypeInfoDTO.RuleFor(p => p.Max, s => s.Random.Double());
        cellDataTypeInfoDTO.RuleFor(p => p.DateFormats, s => s.Random.AlphaNumeric(7));
        cellDataTypeInfoDTO.RuleFor(p => p.CustomValidation, s => s.Random.AlphaNumeric(7));
        cellDataTypeInfoDTO.Generate();

        var columnDefinitionDTO = new Faker<ColumnDefinitionDTO>();
        columnDefinitionDTO.RuleFor(p => p.OrderNumber, s => s.Random.Int());
        columnDefinitionDTO.RuleFor(p => p.TargetFieldName, s => s.Random.AlphaNumeric(7));
        columnDefinitionDTO.RuleFor(p => p.Type, s => new CellDataType());
        columnDefinitionDTO.RuleFor(p => p.TypeInfo, s => new CellDataTypeInfoDTO());
        columnDefinitionDTO.RuleFor(p => p.Position, s => s.Random.Int());
        columnDefinitionDTO.RuleFor(p => p.IsAllowNulls, s => s.Random.Bool());
        columnDefinitionDTO.RuleFor(p => p.DefaultValue, s => s.Random.AlphaNumeric(7));
        columnDefinitionDTO.Generate();

        var mockStream = new Mock<Stream>();

        var dataImportConfigDTO = new Faker<DataImportConfigDTO>();
        dataImportConfigDTO.RuleFor(p => p.FirstRow, s => s.Random.Int());
        dataImportConfigDTO.RuleFor(p => p.LastRow, s => s.Random.Int());
        dataImportConfigDTO.RuleFor(p => p.SheetName, s => s.Random.AlphaNumeric(7));
        dataImportConfigDTO.RuleFor(p => p.Data, s => new JsonObject());
        dataImportConfigDTO.RuleFor(p => p.MaxErrorsCount, s => s.Random.Int());
        dataImportConfigDTO.RuleFor(p => p.ColumnDefinitions, s => new List<ColumnDefinitionDTO>() { });
        dataImportConfigDTO.RuleFor(p => p.FileName, s => s.Random.AlphaNumeric(7));
        dataImportConfigDTO.RuleFor(p => p.FileStream, s => mockStream.Object);
        dataImportConfigDTO.RuleFor(p => p.UserId, s => s.Random.AlphaNumeric(7));
        dataImportConfigDTO.RuleFor(p => p.SkipInvalidRows, s => s.Random.Bool());
        dataImportConfigDTO.Generate();

        var mockIWorkbook = new Mock<IWorkbook>();

        var dataImportResultDTO = new Faker<DataImportResultDTO>();
        dataImportResultDTO.RuleFor(p => p.Warning, s => s.Random.AlphaNumeric(7));
        dataImportResultDTO.RuleFor(p => p.InvalidEntries, s => new List<ImportEntryDTO>() { });
        dataImportResultDTO.RuleFor(p => p.ImportedCount, s => s.Random.Int());
        dataImportResultDTO.RuleFor(p => p.ExcelResult, s => mockIWorkbook.Object);
        dataImportResultDTO.RuleFor(p => p.FileName, s => s.Random.AlphaNumeric(7));
        dataImportResultDTO.Generate();

        var importEntryCellDTO = new Faker<ImportEntryCellDTO>();
        importEntryCellDTO.RuleFor(p => p.TargetFieldName, s => s.Random.AlphaNumeric(7));
        importEntryCellDTO.RuleFor(p => p.ErrorMessage, s => s.Random.AlphaNumeric(7));
        importEntryCellDTO.RuleFor(p => p.OrderNumber, s => s.Random.Int());
        importEntryCellDTO.RuleFor(p => p.Type, s => s.Random.Int());
        importEntryCellDTO.RuleFor(p => p.Value, s => new object());
        importEntryCellDTO.Generate();

        var importEntryDTO = new Faker<ImportEntryDTO>();
        importEntryDTO.RuleFor(p => p.LineNumber, s => s.Random.Int());
        importEntryDTO.RuleFor(p => p.Cells, s => new List<ImportEntryCellDTO>() { });
        importEntryDTO.RuleFor(p => p.ErrorMessage, s => s.Random.AlphaNumeric(7));
        importEntryDTO.Generate();

        var importSettings = new Faker<ImportSettings>();
        importSettings.RuleFor(p => p.File, s => mockStream.Object);
        importSettings.RuleFor(p => p.FileName, s => s.Random.AlphaNumeric(7));
        importSettings.RuleFor(p => p.Config, s => new DataImportConfigDTO());
        importSettings.Generate();
    }
}