using BBWM.Core.Autofac;
using BBWM.DataProcessing.Classes;

namespace BBWM.DataProcessing.Validation;

/// <summary>
/// Validates the cell by custom logic
/// </summary>
public class CustomValidator : EntryCellValidator
{
    private readonly CustomValidationHandler _handler;

    /// <summary>
    /// Custom construtctor
    /// </summary>
    /// <param name="typeInfo">type info</param>
    public CustomValidator(CustomCellDataTypeInfo typeInfo = null)
    {
        if (typeInfo is null) return;
        _handler = typeInfo.CustomValidation;
    }

    /// <summary>
    /// Perform validation for the cell value 
    /// </summary>
    /// <param name="cell">cell</param>
    [IgnoreLogging]
    public override void PerformValidation(ImportEntryCell cell)
    {
        _handler?.Invoke(cell);
    }
}
