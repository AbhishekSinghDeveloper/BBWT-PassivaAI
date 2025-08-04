using BBWM.DataProcessing.Classes;

using Xunit;

namespace BBWM.DataProcessing.Test.Classes;

public class DataImportResultTests
{
    private static readonly ImportEntry invalidEntry = new(new object[0])
    {
        ErrorMessage = "Error",
        Cells = new List<ImportEntryCell> { new(1, new()) { ErrorMessage = "Error" } },
    };

    private static readonly ImportEntry validEntry = new(new object[0])
    {
        Cells = new List<ImportEntryCell> { new(1, new()) },
    };

    [Fact]
    public void InvalidEntries()
    {
        // Arrange
        DataImportResult dataImportResult = CreateDataImportResult();

        // Act
        List<ImportEntry> invalidEntries = dataImportResult.InvalidEntries;

        // Assert
        Assert.Single(invalidEntries);
    }

    [Fact]
    public void ValidEntries()
    {
        // Arrange
        DataImportResult dataImportResult = CreateDataImportResult();

        // Act
        IEnumerable<ImportEntry> validEntries = dataImportResult.ValidEntries;

        // Assert
        Assert.Single(validEntries);
    }

    private static DataImportResult CreateDataImportResult() => new(new[] { invalidEntry, validEntry });
}
