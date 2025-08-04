namespace BBWM.DataProcessing.FileReaders;

/// <summary>
/// IDataImportReader
/// </summary>
public interface IDataImportReader
{
    /// <summary>
    /// Reads the file and returns rows enumerable
    /// </summary>
    /// <param name="fileStream">File stream</param>
    /// <param name="firstRow">The first row from which a file is read</param>
    /// <param name="lastRow">The last row (optional)</param>
    /// <param name="sheetName">Sheet Name</param>
    /// <returns>Rows enumerable</returns>
    IEnumerable<object[]> ReadFile(Stream fileStream, int firstRow, int? lastRow, string sheetName);
}
