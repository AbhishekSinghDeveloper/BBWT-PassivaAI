namespace BBWM.DataProcessing.DTO;

/// <summary>
/// DTO for ImportEntry
/// </summary>
public class ImportEntryDTO
{
    /// <summary>
    /// Line number
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Cells
    /// </summary>
    public List<ImportEntryCellDTO> Cells { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string ErrorMessage { get; set; }
}
