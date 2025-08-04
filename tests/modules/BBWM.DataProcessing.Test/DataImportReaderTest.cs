using BBWM.DataProcessing.FileReaders;

using Xunit;

namespace BBWM.DataProcessing.Test;

public abstract class DataImportReaderTest
{
    protected DataImportReaderTest()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    protected abstract Stream GetContentStream();

    protected abstract IDataImportReader GetReader();

    [Fact]
    public void Get_Count()
    {
        // Arrange
        var stream = GetContentStream();
        var reader = GetReader();
        var firstRow = 0;
        int? lastRow = null;
        string sheetName = null;

        // Act
        var result = reader.ReadFile(stream, firstRow, lastRow, sheetName);

        // Assert
        Assert.Equal(6, result.Count());
    }

    [Fact]
    public void Get_Count_From_Second_Row()
    {
        // Arrange
        var stream = GetContentStream();
        var reader = GetReader();
        var firstRow = 2;
        int? lastRow = null;
        string sheetName = null;

        // Act
        var result = reader.ReadFile(stream, firstRow, lastRow, sheetName);

        // Assert
        Assert.Equal(5, result.Count());
    }
}
