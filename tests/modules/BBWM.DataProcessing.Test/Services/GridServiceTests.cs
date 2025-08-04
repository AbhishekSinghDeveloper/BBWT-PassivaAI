using BBWM.DataProcessing.Services;

using Bogus;

using NPOI.SS.UserModel;

using Xunit;

namespace BBWM.DataProcessing.Test.Services;

public class GridServiceTests
{
    [Fact]
    public void PrintExcel()
    {
        // Arrange
        GridData gridData = CreateGridData();
        GridService gridService = new();
        IEnumerable<GridDataItemFake> data = CreateGridRows(10);

        // Act
        byte[] excelBytes = gridService.PrintExcel(data, gridData);

        // Assert
        using MemoryStream excelStream = new MemoryStream(excelBytes);
        Exception createException = Record.Exception(() => WorkbookFactory.Create(excelStream));
        Assert.Null(createException);
    }

    [Fact]
    public void PrintCSV()
    {
        // Arrange
        GridData gridData = CreateGridData();
        GridService gridService = new();
        IEnumerable<GridDataItemFake> data = CreateGridRows(10);

        // Act
        byte[] csvBytes = gridService.PrintCSV(data, gridData);

        // Assert
        using StreamReader csvStream = new(new MemoryStream(csvBytes));
        string[] csvLines = csvStream.ReadToEnd().Trim().Split("\r\n");
        Assert.Equal(11, csvLines.Length);
    }

    private static GridData CreateGridData()
        => new()
        {
            GridTableColumns = new List<GridTableColumn>
            {
                new GridTableColumn
                {
                    Field = "id_original",
                    Header = nameof(GridDataItemFake.Id),
                },
                new GridTableColumn
                {
                    Field = nameof(GridDataItemFake.Name),
                    Header = nameof(GridDataItemFake.Name),
                },
                new GridTableColumn
                {
                    SortField = nameof(GridDataItemFake.Epsilon),
                    Header = nameof(GridDataItemFake.Epsilon),
                },
                new GridTableColumn
                {
                    Field = nameof(GridDataItemFake.LastAccess),
                    Header = nameof(GridDataItemFake.LastAccess),
                },
            },
        };

    private static IEnumerable<GridDataItemFake> CreateGridRows(int count)
        => new Faker<GridDataItemFake>()
            .RuleFor(i => i.Name, f => f.Random.Bool() ? f.Random.AlphaNumeric(10) : null)
            .RuleFor(i => i.Epsilon, f => f.Random.Double())
            .RuleFor(i => i.LastAccess, f => new DateTime(DateTime.UtcNow.Ticks - f.Random.Long(1, 100_000_000)))
            .Generate(count);

    private class GridDataItemFake
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public double Epsilon { get; set; }

        public DateTime LastAccess { get; set; }
    }
}
