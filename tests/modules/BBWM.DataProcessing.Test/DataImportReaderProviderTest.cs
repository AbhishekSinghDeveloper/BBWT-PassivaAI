using BBWM.DataProcessing.FileReaders;

using Xunit;

namespace BBWM.DataProcessing.Test;

public class DataImportReaderProviderTest
{
    [Fact]
    public void GetReader_Should_Throw_On_Invalid_Extension()
    {
        // Arrange
        var provider = new DataImportReaderProvider();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => provider.GetReader(".unk"));
    }

    [Theory]
    [InlineData(".csv")]
    [InlineData(".tsv")]
    public void Get_Csv_Reader(string fileExtension)
    {
        // Arrange
        var provider = new DataImportReaderProvider();

        // Act
        var reader = provider.GetReader(fileExtension);

        // Assert
        Assert.IsType<CSVFileReader>(reader);
    }

    [Fact]
    public void Get_Xls_Reader()
    {
        // Arrange
        var provider = new DataImportReaderProvider();

        // Act
        var reader = provider.GetReader(".xls");

        // Assert
        Assert.IsType<XlsFileReader>(reader);
    }

    [Fact]
    public void Get_Xlsx_Reader()
    {
        // Arrange
        var provider = new DataImportReaderProvider();

        // Act
        var reader = provider.GetReader(".xlsx");

        // Assert
        Assert.IsType<XlsxFileReader>(reader);
    }
}
