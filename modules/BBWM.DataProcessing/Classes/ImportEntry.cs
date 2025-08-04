namespace BBWM.DataProcessing.Classes;

/// <summary>
/// The data import entry
/// </summary>
public class ImportEntry
{
    /// <summary>
    /// Custom constructor
    /// </summary>
    /// <param name="originalData">Original row's cells array</param>
    public ImportEntry(object[] originalData)
    {
        OriginalData = originalData;
        Cells = new List<ImportEntryCell>(originalData.Length);
    }

    /// <summary>
    /// Line number
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Original row's cells array
    /// </summary>
    public object[] OriginalData { get; set; }

    /// <summary>
    /// The <see cref="ImportEntryCell"/>'s List
    /// </summary>
    public List<ImportEntryCell> Cells { get; set; }

    /// <summary>
    /// The whole entry's validation error message
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Determines if the entry is valid
    /// </summary>
    public bool IsValid
    {
        get
        {
            return string.IsNullOrWhiteSpace(ErrorMessage) && Cells.All(c => c.IsValid);
        }
    }
}
