using BBWM.DataProcessing.Export;

using Xunit;

using static BBWM.DataProcessing.Test.Export.CsvExportHelper;

namespace BBWM.DataProcessing.Test.Export;

public class ReportCSVServiceTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void GetCsvFile(bool includeColumnSeparatorDefinitionPreamble, bool includeFooter)
    {
        // Arrange
        CsvExportFakeClass[] rows = GetRows(10).ToArray();
        ReportCSVService<CsvExportFakeClass> reportCSVService = new();

        List<ColumnSetting<CsvExportFakeClass>> columnSettings =
            typeof(CsvExportFakeClass)
                .GetProperties()
                .Select(
                    p => new ColumnSetting<CsvExportFakeClass>
                    {
                        Header = p.Name,
                        GetValue = obj => p.GetValue(obj).ToString(),
                    })
                .ToList();

        CsvExportFakeClass footerRow = new() { Id = 11, FirstName = "Cool" };
        Action<List<ColumnSetting<CsvExportFakeClass>>, CsvExport, List<CsvExportFakeClass>> footer =
            includeFooter
                ? (columnSettings, exporter, data) =>
                {
                    exporter[nameof(CsvExportFakeClass.Id)] = footerRow.Id;
                    exporter[nameof(CsvExportFakeClass.FirstName)] = footerRow.FirstName;
                }
        : null;

        // Act
        CsvExport exporter = reportCSVService.GetCsvFile(
            rows.ToList(),
            columnSettings,
            footer,
            includeColumnSeparatorDefinitionPreamble: includeColumnSeparatorDefinitionPreamble);

        // Assert
        byte[] bytes = exporter.ExportToBytes(false);
        CsvExportFakeClass[] expectedRows = includeFooter ? rows.Concat(new[] { footerRow }).ToArray() : rows;
        AssertExport(new MemoryStream(bytes), includeColumnSeparatorDefinitionPreamble, true, expectedRows, 2);
    }
}
