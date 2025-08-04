using BBWM.Core.Autofac;
using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.FileReaders;
using BBWM.DataProcessing.Validation;

namespace BBWM.DataProcessing.Services;

/// <summary>
/// Data import helper implementation
/// </summary>
public class DataImportHelper : IDataImportHelper
{
    /// <summary>
    /// IDataImportReaderProvider instance
    /// </summary>
    private readonly IDataImportReaderProvider _dataImportReaderProvider;
    private readonly ITypeValidatorsProvider _typeValidatorsProvider;


    /// <summary>
    /// Custom constructor
    /// </summary>
    /// <param name="dataImportReaderProvider"></param>
    public DataImportHelper(IDataImportReaderProvider dataImportReaderProvider, ITypeValidatorsProvider typeValidatorsProvider)
    {
        _dataImportReaderProvider = dataImportReaderProvider;
        _typeValidatorsProvider = typeValidatorsProvider;
    }

    #region Import

    /// <summary>
    /// Processes the data import from scv,xls,xlsx file stream
    /// </summary>
    /// <param name="config">Import configuration</param>
    /// <param name="onEntryProcessedCallback">Callback which is called on each processed entry</param>
    [IgnoreLogging]
    public DataImportResult ProcessDataImport(DataImportConfig config, OnEntryProcessedCallback onEntryProcessedCallback = null)
    {
        if (config is null)
            throw new ArgumentNullException("config is null");
        if (config.FileStream is null)
            throw new ArgumentException("config.FileStream is null");
        if (string.IsNullOrWhiteSpace(config.FileName))
            throw new ArgumentException("config.FileName is null or empty");

        if (config.FileStream.Length == 0)
            return new DataImportResult("Your file is empty");

        var extension = Path.GetExtension(config.FileName);
        try
        {
            byte[] StreamByte = null;
            StreamByte = ((MemoryStream)config.FileStream).ToArray();

            var reader = _dataImportReaderProvider.GetReader(extension);
            var parsedRows = reader.ReadFile(new MemoryStream(StreamByte), config.FirstRow, config.LastRow, config.SheetName);

            var entries = ProcessDataImportInternal(parsedRows, config, onEntryProcessedCallback);
            var result = new DataImportResult(entries.ToList());

            return result;
        }
        catch (ArgumentException e)
        {
            return new DataImportResult(e.Message);
        }
    }

    /// <summary>
    /// Processes the data import from enumerable with parsed rows
    /// </summary>
    /// <param name="parsedRows">Parsed rows enumerable</param>
    /// <param name="config">Import configuration</param>
    /// <param name="onEntryProcessedCallback">Callback which is called on each processed entry</param>
    /// <returns></returns>
    private IEnumerable<ImportEntry> ProcessDataImportInternal(IEnumerable<object[]> parsedRows, DataImportConfig config, OnEntryProcessedCallback onEntryProcessedCallback)
    {
        var errorsCount = 0;
        var currentRow = config.FirstRow;
        foreach (var row in parsedRows)
        {
            var entry = ValidateRow(row, config.ColumnDefinitions);
            entry.LineNumber = currentRow++;

            if (!entry.IsValid)
            {
                if (config.SkipInvalidRows)
                    continue;

                errorsCount++;
            }

            if (RaiseOnEntryProcessedCallback(entry, onEntryProcessedCallback))
                yield break;

            if (config.MaxErrorsCount.HasValue && errorsCount > config.MaxErrorsCount)
                yield break;

            yield return entry;
        }

    }

    /// <summary>
    /// Raises OnEntryProcessedCallback
    /// </summary>
    /// <param name="entry">Entry</param>
    /// <param name="callback">Callback instance</param>
    /// <returns>If true, the user wants to stop processing</returns>
    private static bool RaiseOnEntryProcessedCallback(ImportEntry entry, OnEntryProcessedCallback callback)
    {
        if (callback is not null)
        {
            var args = new OnEntryProcessedArgs(entry);
            callback.Invoke(args);
            return args.IsProcessStopped;
        }
        return false;
    }

    #endregion

    #region Validation

    /// <summary>
    /// Performs the row validation and returns ImportEntry instance
    /// </summary>
    /// <param name="row">The row's cells array</param>
    /// <param name="columnDefinitions">column definitions</param>
    /// <returns>ImportEntry intance</returns>
    private ImportEntry ValidateRow(object[] row, ColumnsDefinitionsCollection columnDefinitions)
    {
        var result = new ImportEntry(row);

        if (row.Length <= columnDefinitions.Max(x => x.Position) - 1)
        {
            result.ErrorMessage = "The row contains invalid cells amount";
            return result;
        }

        for (var i = 0; i < row.Length && i < columnDefinitions.Count; i++)
        {
            var colDef = columnDefinitions.ElementAt(i);
            var res = ValidateCell(row[colDef.Position - 1], colDef);
            result.Cells.Add(res);
        }

        return result;
    }

    /// <summary>
    /// Performs the cell validation and returns ImportEntryCell instance
    /// </summary>
    /// <param name="cellValue">Cell value</param>
    /// <param name="columnDefinition">Column definition</param>
    /// <returns>ImportEntryCell instance</returns>
    [IgnoreLogging]
    private ImportEntryCell ValidateCell(object cellValue, ColumnDefinition columnDefinition)
    {
        var result = new ImportEntryCell(cellValue, columnDefinition);

        if (CellValueIsNullOrEmpty(result.Value))
        {
            if (!columnDefinition.IsAllowNulls)
            {
                result.ErrorMessage = "Null value is not allowed";
            }
            else if (columnDefinition.DefaultValue is not null)
            {
                result.Value = columnDefinition.DefaultValue;
            }
        }
        else
        {
            _typeValidatorsProvider.GetValidator(columnDefinition).PerformValidation(result);
        }

        return result;
    }

    /// <summary>
    /// Determines whether the value of the cell is null or empty
    /// </summary>
    /// <param name="cellValue">ImportEntryCell instance</param>
    /// <returns></returns>
    private static bool CellValueIsNullOrEmpty(object cellValue)
    {
        return string.IsNullOrEmpty(Convert.ToString(cellValue));
    }
    #endregion
}
