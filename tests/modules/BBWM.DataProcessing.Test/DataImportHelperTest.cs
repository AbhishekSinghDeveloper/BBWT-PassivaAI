using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.FileReaders;
using BBWM.DataProcessing.Services;
using BBWM.DataProcessing.Validation;

using Moq;

using System.Text;

using Xunit;

namespace BBWM.DataProcessing.Test;

public class DataImportHelperTest
{
    [Fact]
    public void Fail_If_Config_Is_Null()
    {
        // arrange
        var helper = new DataImportHelper(null, null);

        // act
        // assert
        Assert.Throws<ArgumentNullException>(() => helper.ProcessDataImport(null));
    }

    [Fact]
    public void Fail_If_FileStream_Is_Null()
    {
        // arrange
        var helper = new DataImportHelper(null, null);
        var config = new DataImportConfig { FileStream = null };

        // act
        // assert
        Assert.Throws<ArgumentException>(() => helper.ProcessDataImport(config));
    }

    [Fact]
    public void Fail_If_FileName_Is_Null()
    {
        // arrange
        var helper = new DataImportHelper(null, null);
        var config = new DataImportConfig { FileStream = new MemoryStream(), FileName = null };

        // act
        // assert
        Assert.Throws<ArgumentException>(() => helper.ProcessDataImport(config));
    }

    [Fact]
    public void Return_If_FileStream_Is_Empty()
    {
        // arrange
        var helper = new DataImportHelper(null, null);
        var config = new DataImportConfig { FileStream = new MemoryStream(), FileName = "test.csv" };

        // act
        var res = helper.ProcessDataImport(config);

        // assert
        Assert.NotNull(res);
        Assert.False(string.IsNullOrEmpty(res.Warning));
        Assert.Null(res.Result);
    }

    [Fact]
    public void Result_Basic()
    {
        // arrange
        var data = new List<object[]>
            {
                new object[] { "test1", 1, DateTime.Today },
                new object[] { "test2", 2, DateTime.Today.AddDays(1) },
                new object[] { "test3", 3, DateTime.Today.AddDays(2) },
                new object[] { "test4", 4, DateTime.Today.AddDays(3) },
                new object[] { "test5", 5, DateTime.Today.AddDays(4) },
            };
        var config = CreateDataImportConfig();
        var helper = BuildHelper(data);

        // act
        var res = helper.ProcessDataImport(config);

        // assert
        Assert.NotNull(res);
        Assert.NotEmpty(res.Result);
        Assert.Empty(res.InvalidEntries);
    }

    //[Fact]
    //public void Result_If_Columns_Count_Is_Invalid()
    //{
    //    // arrange
    //    var data = new List<object[]>
    //    {
    //        new object[] { "test1", 1, DateTime.Today, "extended" }
    //    };
    //    var config = CreateDataImportConfig();
    //    var helper = BuildHelper(data);

    //    // act
    //    var res = helper.ProcessDataImport(config);

    //    // assert
    //    Assert.NotNull(res);
    //    Assert.NotEmpty(res.Result);
    //    Assert.NotEmpty(res.InvalidEntries);
    //    Assert.Contains(res.InvalidEntries, entry => entry.ErrorMessage.Contains("invalid cells amount"));
    //}

    [Fact]
    public void Result_If_Cell_Value_Is_Not_Allows_Nulls()
    {
        // arrange
        var data = new List<object[]>
            {
                new object[] { "test1", null, DateTime.Today },
            };
        var config = CreateDataImportConfig();
        config.ColumnDefinitions.ElementAt(1).IsAllowNulls = false;

        var helper = BuildHelper(data);

        // act
        var res = helper.ProcessDataImport(config);

        // assert
        Assert.NotNull(res);
        Assert.NotEmpty(res.Result);
        Assert.NotEmpty(res.InvalidEntries);
        Assert.Equal("Null value is not allowed", res.InvalidEntries.First().Cells[1].ErrorMessage);
    }

    [Fact]
    public void Result_With_Default_Value()
    {
        // arrange
        var defaultValue = 1024;
        var data = new List<object[]>
            {
                new object[] { "test1", null, DateTime.Today },
            };
        var config = CreateDataImportConfig();
        config.ColumnDefinitions.ElementAt(1).DefaultValue = defaultValue;

        var helper = BuildHelper(data);

        // act
        var res = helper.ProcessDataImport(config);

        // assert
        Assert.NotNull(res);
        Assert.NotEmpty(res.Result);
        Assert.Empty(res.InvalidEntries);
        Assert.Equal(res.Result.First().Cells[1].Value, defaultValue);
    }

    [Fact]
    public void Result_With_Max_Errors_Count()
    {
        // arrange
        var maxErrorsCount = 3;
        var data = new List<object[]>
            {
                new object[] { "test1", null, DateTime.Today },
                new object[] { "test2", null, DateTime.Today.AddDays(1) },
                new object[] { "test3", null, DateTime.Today.AddDays(2) },
                new object[] { "test4", null, DateTime.Today.AddDays(3) },
                new object[] { "test5", null, DateTime.Today.AddDays(4) },
            };
        var config = CreateDataImportConfig();
        config.ColumnDefinitions.ElementAt(1).IsAllowNulls = false;
        config.MaxErrorsCount = maxErrorsCount;

        var helper = BuildHelper(data);

        // act
        var res = helper.ProcessDataImport(config);

        // assert
        Assert.NotNull(res);
        Assert.NotEmpty(res.Result);
        Assert.NotEmpty(res.InvalidEntries);
        Assert.Equal(maxErrorsCount, res.InvalidEntries.Count());
    }

    [Fact]
    public void Result_If_Stop_Via_Callback()
    {
        // arrange
        var data = new List<object[]>
            {
                new object[] { "test1", 1, DateTime.Today },
                new object[] { "test2", 2, DateTime.Today.AddDays(1) },
                new object[] { "test3", 3, DateTime.Today.AddDays(2) },
                new object[] { "test4", 4, DateTime.Today.AddDays(3) },
                new object[] { "test5", 5, DateTime.Today.AddDays(4) },
            };
        var config = CreateDataImportConfig();
        var helper = BuildHelper(data);

        // act
        var res = helper.ProcessDataImport(config, args =>
        {
            args.StopImport();
        });

        // assert
        Assert.NotNull(res);
        Assert.Empty(res.Result);
    }

    private static DataImportHelper BuildHelper(IEnumerable<object[]> data)
    {
        var readerMock = new Mock<IDataImportReader>();
        var readerProviderMock = new Mock<IDataImportReaderProvider>();
        var validatorsProviderMock = new Mock<ITypeValidatorsProvider>();

        readerMock.Setup(r => r.ReadFile(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<string>())).Returns(data);
        readerProviderMock.Setup(a => a.GetReader(It.IsAny<string>())).Returns(readerMock.Object);

        validatorsProviderMock.Setup(a => a.GetValidator(It.IsAny<ColumnDefinition>())).Returns(new StringValidator());

        var dataImportReaderProvider = readerProviderMock.Object;
        var typesValidatorsProvider = validatorsProviderMock.Object;

        return new DataImportHelper(dataImportReaderProvider, typesValidatorsProvider);
    }

    private static DataImportConfig CreateDataImportConfig()
    {
        return new DataImportConfig
        {
            FileStream = new MemoryStream(Encoding.UTF8.GetBytes("test")),
            FileName = "test.csv",
            FirstRow = 0,
            MaxErrorsCount = 10,
            ColumnDefinitions = new ColumnsDefinitionsCollection(
                new ColumnDefinition { OrderNumber = 1, TargetFieldName = "Field 1", Type = CellDataType.String, IsAllowNulls = true, Position = 1 },
                new ColumnDefinition { OrderNumber = 2, TargetFieldName = "Field 2", Type = CellDataType.Number, IsAllowNulls = true, Position = 2 },
                new ColumnDefinition { OrderNumber = 3, TargetFieldName = "Field 3", Type = CellDataType.Date, IsAllowNulls = true, Position = 3 }),
        };
    }
}
