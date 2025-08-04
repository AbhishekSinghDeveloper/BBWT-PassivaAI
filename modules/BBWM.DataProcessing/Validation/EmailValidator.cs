using BBWM.DataProcessing.Classes;

using System.ComponentModel.DataAnnotations;

namespace BBWM.DataProcessing.Validation;

/// <summary>
/// Validates the cell for email address
/// </summary>
public class EmailValidator : EntryCellValidator
{
    public override void PerformValidation(ImportEntryCell cell)
    {
        if (!new EmailAddressAttribute().IsValid(cell.Value))
        {
            cell.ErrorMessage = "Invalid email address";
        }
    }
}
