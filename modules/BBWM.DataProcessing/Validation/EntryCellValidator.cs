using BBWM.DataProcessing.Classes;

namespace BBWM.DataProcessing.Validation;

/// <summary>
/// Base abstract class for performing validation
/// </summary>
public abstract class EntryCellValidator
{
    /// <summary>
    /// Perform validation for the cell value 
    /// </summary>
    /// <param name="cell">cell</param>
    public abstract void PerformValidation(ImportEntryCell cell);
}
