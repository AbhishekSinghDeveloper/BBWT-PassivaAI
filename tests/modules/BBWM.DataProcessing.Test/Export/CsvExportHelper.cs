using Bogus;
using Xunit;

namespace BBWM.DataProcessing.Test.Export;

internal static class CsvExportHelper
{
    public static void AssertExport(
        Stream stream,
        bool includeColumnSeparatorDefinitionPreamble,
        bool includeHeader,
        CsvExportFakeClass[] rows,
        int expectedHeadersCount,
        string[] expectedHeaders = default)
    {
        using StreamReader exportedStream = new(stream, true);
        string[] exportedLines = exportedStream.ReadToEnd().Trim().Split(Environment.NewLine);

        int totalLines = rows.Length + (includeColumnSeparatorDefinitionPreamble ? 1 : 0) + (includeHeader ? 1 : 0);

        Assert.Equal(totalLines, exportedLines.Length);

        if (includeColumnSeparatorDefinitionPreamble)
            Assert.Equal("sep=,", exportedLines[0]);

        if (includeHeader)
        {
            string[] actualHeaders = exportedLines[includeColumnSeparatorDefinitionPreamble ? 1 : 0].Split(",");
            expectedHeaders ??= typeof(CsvExportFakeClass).GetProperties().Select(pi => $"\"{pi.Name}\"").ToArray();
            Assert.Equal(expectedHeadersCount, actualHeaders.Length);
            Assert.All(expectedHeaders, p => Assert.Contains(p, actualHeaders));
        }

        // Check we have the correct (unordered) data
        int dataStartIndex = 0 + (includeColumnSeparatorDefinitionPreamble ? 1 : 0) + (includeHeader ? 1 : 0);
        for (int lineIndex = dataStartIndex, rowIndex = 0; lineIndex < exportedLines.Length; lineIndex++, rowIndex++)
        {
            string[] expectedValues = new[]
            {
                rows[rowIndex].Id.ToString(),
                rows[rowIndex].FirstName,
            }
            .Select(x => $"\"{x}\"").ToArray();

            string[] actualValues = exportedLines[lineIndex].Split(",");

            Assert.All(expectedValues, v => Assert.Contains(v, actualValues));
        }
    }

    public static IEnumerable<CsvExportFakeClass> GetRows(int count)
    {
        int id = 1;
        return new Faker<CsvExportFakeClass>()
            .RuleFor(o => o.Id, _ => id++)
            .RuleFor(o => o.FirstName, f => f.Random.AlphaNumeric(7))
            .Generate(count);
    }
}
