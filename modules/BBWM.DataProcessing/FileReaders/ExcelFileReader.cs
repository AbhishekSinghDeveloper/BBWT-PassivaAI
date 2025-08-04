using ExcelDataReader;

using System.Text;

namespace BBWM.DataProcessing.FileReaders;

/// <summary>
/// IDataImportReader implementation and base class for excel readers
/// </summary>
public abstract class ExcelFileReader : IDataImportReader
{
    /// <summary>
    /// Reads the file and returns rows enumerable
    /// </summary>
    /// <param name="fileStream">File stream</param>
    /// <param name="firstRow">The first row from which a file is read</param>
    /// <param name="lastRow">The last row (optional)</param>
    /// <param name="sheetName">Sheet Name</param>
    /// <returns>Rows enumerable</returns>
    public IEnumerable<object[]> ReadFile(Stream fileStream, int firstRow, int? lastRow, string sheetName)
    {
        var excelReader = CreateExcelDataReader(fileStream);
        return ParseExcelContent(excelReader, firstRow, lastRow, sheetName);
    }

    /// <summary>
    /// The template method, which parces excel file
    /// </summary>
    /// <param name="excelReader"></param>
    /// <param name="firstRow"></param>
    /// <param name="lastRow">The last row (optional)</param>
    /// <param name="sheetName">Sheet Name</param>
    /// <returns></returns>
    protected virtual IEnumerable<object[]> ParseExcelContent(IExcelDataReader excelReader, int firstRow, int? lastRow, string sheetName)
    {
        if (!string.IsNullOrEmpty(sheetName))
        {
            do
            {
                if (excelReader.Name == sheetName)
                    break;
            }
            while (excelReader.NextResult());
        }

        for (var currentRow = 1; excelReader.Read(); currentRow++)
        {
            if (currentRow < firstRow) continue;
            if (lastRow.HasValue && currentRow > lastRow) break;

            var row = new object[excelReader.FieldCount];

            for (var i = 0; i < excelReader.FieldCount; i++)
                row[i] = excelReader.GetValue(i);

            yield return row;
        }
    }

    /// <summary>
    /// Creates IExcelDataReader instance
    /// </summary>
    /// <param name="fileStream">File stream</param>
    /// <returns>IExcelDataReader instance</returns>
    protected abstract IExcelDataReader CreateExcelDataReader(Stream fileStream);
}

/// <summary>
/// ExcelFileReader's subclass which reads xls files
/// </summary>
public class XlsFileReader : ExcelFileReader
{
    protected override IExcelDataReader CreateExcelDataReader(Stream fileStream)
    {
        return ExcelReaderFactory.CreateBinaryReader(fileStream, new ExcelReaderConfiguration
        {
            FallbackEncoding = Encoding.GetEncoding(1252)
        });
    }
}

/// <summary>
/// ExcelFileReader's subclass which reads xlsx files
/// </summary>
public class XlsxFileReader : ExcelFileReader
{
    protected override IExcelDataReader CreateExcelDataReader(Stream fileStream)
    {
        return ExcelReaderFactory.CreateOpenXmlReader(fileStream);
    }
}
