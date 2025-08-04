namespace BBWM.DataProcessing.FileReaders;

/// <summary>
/// IDataImportReaderProvider
/// </summary>
public interface IDataImportReaderProvider
{
    /// <summary>
    /// Returns IDataImportReader instance by file's extension
    /// </summary>
    /// <param name="fileExtension">file's extension</param>
    /// <returns>IDataImportReader instance</returns>
    IDataImportReader GetReader(string fileExtension);
}
