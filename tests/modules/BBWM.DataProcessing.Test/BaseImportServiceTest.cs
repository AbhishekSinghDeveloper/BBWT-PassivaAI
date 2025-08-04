using AutoMapper;

using BBWM.Core.Test;
using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.DTO;
using BBWM.DataProcessing.FileReaders;
using BBWM.DataProcessing.Services;
using BBWM.DataProcessing.Validation;
using BBWT.Tests.modules.BBWM.DataProcessing.Test;
using Microsoft.AspNetCore.SignalR;

using Moq;

using Xunit;

namespace BBWM.DataProcessing.Test;

public class BaseImportServiceTest
{
    [Fact(Skip = "Mock SignalR IHubContext")]
    public async Task TestAsync()
    {
        var dataImportConfigDTO = new DataImportConfigDTO
        {
            FirstRow = 2,
            MaxErrorsCount = 10,
            SkipInvalidRows = false,
            ColumnDefinitions = new List<ColumnDefinitionDTO>
                {
                    new ColumnDefinitionDTO
                    {
                        OrderNumber = 1,
                        TargetFieldName = "Name",
                        Type = CellDataType.String,
                        IsAllowNulls = false,
                        TypeInfo = new CellDataTypeInfoDTO
                        {
                            Min = 20,
                            Max = 70,
                            DateFormats ="dd.MM.yyyy, dd/MM/yyyy",
                            CustomValidation = string.Empty,
                        },
                        Position = 1,
                    },
                    new ColumnDefinitionDTO
                    {
                        OrderNumber = 2,
                        TargetFieldName = "Age",
                        Type = CellDataType.Number,
                        IsAllowNulls = false,
                        TypeInfo = new CellDataTypeInfoDTO
                        {
                            Min = 20,
                            Max = 70,
                            DateFormats ="dd.MM.yyyy, dd/MM/yyyy",
                            CustomValidation = string.Empty,
                        },
                        Position = 2,
                    },
                    new ColumnDefinitionDTO
                    {
                        OrderNumber = 3,
                        TargetFieldName = "Phone",
                        Type = CellDataType.Phone,
                        IsAllowNulls = true,
                        TypeInfo = new CellDataTypeInfoDTO
                        {
                            Min = 20,
                            Max = 70,
                            DateFormats ="dd.MM.yyyy, dd/MM/yyyy",
                            CustomValidation = string.Empty,
                        },
                        Position = 3,
                    },
                    new ColumnDefinitionDTO
                    {
                        OrderNumber = 4,
                        TargetFieldName = "Email",
                        Type = CellDataType.Email,
                        IsAllowNulls = true,
                        TypeInfo = new CellDataTypeInfoDTO(),
                        Position = 4,
                    },
                    new ColumnDefinitionDTO
                    {
                        OrderNumber = 5,
                        TargetFieldName = "RegistrationDate",
                        Type = CellDataType.DateTimeOffset,
                        IsAllowNulls = false,
                        TypeInfo = new CellDataTypeInfoDTO
                        {
                            Min = 20,
                            Max = 70,
                            DateFormats ="dd.MM.yyyy, dd/MM/yyyy",
                            CustomValidation = string.Empty,
                        },
                        Position = 5,
                    },
                    new ColumnDefinitionDTO
                    {
                        OrderNumber = 6,
                        TargetFieldName = "JobRole",
                        Type = CellDataType.Custom,
                        IsAllowNulls = true,
                        TypeInfo = new CellDataTypeInfoDTO
                        {
                            CustomValidation = "JobRole",
                        },
                        Position = 6,
                    },
                },
        };

        var myProfile = new ModuleMappingProfile();
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
        var mapper = new Mapper(configuration);

        var dataImportReaderProvider = new DataImportReaderProvider();
        var typeValidatorsProvider = new TypeValidatorsProvider();
        var dataImportHelper = new DataImportHelper(dataImportReaderProvider, typeValidatorsProvider);
        var context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());

        var hub = new Mock<IHubContext<DataImportHub>>();
        var service = new EmployeesImportService(dataImportHelper, context, mapper, hub.Object);
        var memoryStream = new MemoryStream();

        var importDataModel = new ImportDataModel
        {
            Config = dataImportConfigDTO,
            FileName = "Sample1.csv",
        };
        using var file = new FileStream("Content//Sample1.csv", FileMode.Open, FileAccess.Read);
        file.CopyTo(memoryStream);

        // act
        var dataImportResultDto = await service.Import(service.CreateSettings(importDataModel, memoryStream, Guid.NewGuid().ToString()), default);

        // assert
        var length = context.Set<Employee>().Count();

        Assert.Equal(1000, length);
        Assert.Null(dataImportResultDto.FileName);
        Assert.Equal(1000, dataImportResultDto.ImportedCount);
        Assert.Empty(dataImportResultDto.InvalidEntries);
        Assert.Null(dataImportResultDto.Warning);
    }
}
