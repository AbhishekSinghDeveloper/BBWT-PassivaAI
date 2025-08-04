namespace BBWM.DataProcessing.Classes;

/// <summary>
/// Callback which is called on each processed entry
/// </summary>
/// <param name="args"></param>
public delegate void OnEntryProcessedCallback(OnEntryProcessedArgs args);

/// <summary>
/// The OnEntryProcessedCallback's arguments
/// </summary>
public class OnEntryProcessedArgs
{
    /// <summary>
    /// Custom constructor
    /// </summary>
    /// <param name="entry">ImportEntry instance</param>
    public OnEntryProcessedArgs(ImportEntry entry)
    {
        Entry = entry;
    }

    /// <summary>
    /// The processed entry
    /// </summary>
    public ImportEntry Entry { get; private set; }

    /// <summary>
    /// Indicates that the user wanted to stop data import
    /// </summary>
    public bool IsProcessStopped { get; private set; }

    /// <summary>
    /// Stops the data import
    /// </summary>
    public void StopImport()
    {
        IsProcessStopped = true;
    }
}
