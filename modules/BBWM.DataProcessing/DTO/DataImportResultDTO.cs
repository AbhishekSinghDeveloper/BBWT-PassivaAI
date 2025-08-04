using NPOI.SS.UserModel;
using System.Text.Json.Serialization;

namespace BBWM.DataProcessing.DTO;

/// <summary>
/// DTO for DataImportResult
/// </summary>
public class DataImportResultDTO
{
    /// <summary>
    /// The user friendly warning message
    /// </summary>
    public string Warning { get; set; }

    /// <summary>
    /// Invalid entries
    /// </summary>
    public List<ImportEntryDTO> InvalidEntries { get; set; }

    /// <summary>
    /// Imported entries count
    /// </summary>
    public int ImportedCount { get; set; }

    /// <summary>
    /// Excel result
    /// </summary>
    [JsonIgnore]
    public IWorkbook ExcelResult { get; set; }

    /// <summary>
    /// File name
    /// </summary>
    public string FileName { get; set; }
}