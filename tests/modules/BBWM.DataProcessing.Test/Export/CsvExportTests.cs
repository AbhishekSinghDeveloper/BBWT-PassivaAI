using BBWM.DataProcessing.Export;

using System.Data.SqlTypes;

using Xunit;

using static BBWM.DataProcessing.Test.Export.CsvExportHelper;

namespace BBWM.DataProcessing.Test.Export;

public class CsvExportTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ExportToBytes(bool includeColumnSeparatorDefinitionPreamble, bool includeHeader)
    {
        // Arrange
        var (exporter, rows) = CreateCsvExporterWithData(includeColumnSeparatorDefinitionPreamble);

        // Act
        byte[] bytes = exporter.ExportToBytes(includeHeader);

        // Assert
        AssertExport(new MemoryStream(bytes), includeColumnSeparatorDefinitionPreamble, includeHeader, rows, 3);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ExportToFile(bool includeColumnSeparatorDefinitionPreamble, bool includeHeader)
    {
        // Arrange
        var (exporter, rows) = CreateCsvExporterWithData(includeColumnSeparatorDefinitionPreamble);
        string outputFilename = Path.GetTempFileName();

        // Act
        exporter.ExportToFile(outputFilename, includeHeader);

        // Assert
        AssertExport(File.OpenRead(outputFilename), includeColumnSeparatorDefinitionPreamble, includeHeader, rows, 3);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Merge(bool includeColumnSeparatorDefinitionPreamble, bool includeHeader)
    {
        // Arrange
        var (exporter1, rows1) = CreateCsvExporterWithData(includeColumnSeparatorDefinitionPreamble);
        var (exporter2, rows2) = CreateCsvExporterWithData(includeColumnSeparatorDefinitionPreamble);

        // Act
        CsvExport merge = exporter1.Merge(exporter2, includeColumnSeparatorDefinitionPreamble: includeColumnSeparatorDefinitionPreamble);
        byte[] bytes = merge.ExportToBytes(includeHeader);

        // Assert
        AssertExport(
            new MemoryStream(bytes),
            includeColumnSeparatorDefinitionPreamble,
            includeHeader,
            rows1.Concat(rows2).ToArray(),
            3,
            new[] { "\"1\"", "\"2\"", "\"3\"" });
    }

    public static IEnumerable<object[]> SanitizeCsvValueTestData => new[]
    {
        new object[] { null, string.Empty },
        new object[] { SqlBoolean.Null, string.Empty },

        new object[] { "Hi there", "\"Hi there\"" },
        new object[] { "Hi, there", "\"Hi, there\"" },
        new object[] { "Hi \"there\"", "\"Hi \"\"there\"\"\"" },
        new object[] { "Hi\nthere", "\"Hi\nthere\"" },
        new object[] { "Hi\rthere", "\"Hi\rthere\"" },
        new object[] { new string('a', 30001), "\"" + new string('a', 30000) + "\"" },
    };

    private static (CsvExport, CsvExportFakeClass[]) CreateCsvExporterWithData(bool includeColumnSeparatorDefinitionPreamble)
    {
        CsvExport exporter = new(includeColumnSeparatorDefinitionPreamble: includeColumnSeparatorDefinitionPreamble);
        CsvExportFakeClass[] rows = GetRows(10).ToArray();

        exporter.AddRows(rows);
        exporter["UnknownField"] = "blah";

        return (exporter, rows);
    }
}
