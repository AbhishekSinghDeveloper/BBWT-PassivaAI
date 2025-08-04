namespace BBWM.DataProcessing.FileReaders;

/// <summary>
/// IDataImportReaderProvider implementation
/// </summary>
public class DataImportReaderProvider : IDataImportReaderProvider
{
    private static readonly Dictionary<string, Func<IDataImportReader>> _extensionsByReaderCreators =
        new Dictionary<string, Func<IDataImportReader>>
        {
            { ".csv", () => new CSVFileReader() },
            { ".tsv", () => new CSVFileReader() },
            { ".xls", () => new XlsFileReader() },
            { ".xlsx", () => new XlsxFileReader() },
        };


    /// <summary>
    /// Returns IDataImportReader instance by file's extension
    /// </summary>
    /// <param name="fileExtension">file's extension</param>
    /// <returns>IDataImportReader instance</returns>
    public IDataImportReader GetReader(string fileExtension)
    {
        var supportedExtensions = _extensionsByReaderCreators.Keys.ToList();
        if (!supportedExtensions.Contains(fileExtension))
        {
            throw new ArgumentException(
                $"Import of file type {fileExtension} is not supported. Files types that we can import are {string.Join(", ", supportedExtensions)}.");
        }

        IDataImportReader result = null;
        Func<IDataImportReader> readerCreator;
        if (_extensionsByReaderCreators.TryGetValue(fileExtension, out readerCreator))
        {
            result = readerCreator.Invoke();
        }
        return result;
    }
}
