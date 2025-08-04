using BBWM.DataProcessing.Classes;

namespace BBWM.DataProcessing.Services;

/// <summary>
/// Data import helper interface
/// </summary>
public interface IDataImportHelper
{
    /// <summary>
    /// Imports data from scv,xls,xlsx file stream
    /// </summary>
    /// <param name="config">Import configuration</param>
    /// <param name="onEntryProcessedCallback">Callback which is called on each processed entry</param>
    DataImportResult ProcessDataImport(DataImportConfig config, OnEntryProcessedCallback onEntryProcessedCallback = null);
}
