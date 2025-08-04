using BBWM.DataProcessing.FileReaders;

using Xunit;

namespace BBWM.DataProcessing.Test.ExcelReader;

public abstract class ExcelReaderBase : DataImportReaderTest
{
    [Theory]
    [InlineData(null)]
    [InlineData("data")]
    public virtual void ReadFile(string sheetName)
    {
        // Arrange
        ExcelFileReader reader = GetReader() as ExcelFileReader;

        // Act
        IEnumerable<object[]> result = reader.ReadFile(GetContentStream(), 2, 2, sheetName);

        // Assert
        object[] secondRow = Assert.Single(result);
        Assert.Equal(3, secondRow?.Length ?? 0);
        Assert.Equal("2nd Name", secondRow[0]);
        Assert.Equal(2D, secondRow[1]);
        Assert.Equal("20/07/2011", secondRow[2]);
    }
}
