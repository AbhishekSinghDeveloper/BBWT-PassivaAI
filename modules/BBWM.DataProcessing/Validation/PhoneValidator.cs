using BBWM.DataProcessing.Classes;

using PhoneNumbers;

namespace BBWM.DataProcessing.Validation;

/// <summary>
/// Validates the cell for phone number
/// </summary>
public class PhoneValidator : EntryCellValidator
{
    public override void PerformValidation(ImportEntryCell cell)
    {
        var phoneNumber = Convert.ToString(cell.Value);

        var util = PhoneNumberUtil.GetInstance();

        try
        {
            var phone = util.Parse(phoneNumber, "GB");
            if (!util.IsValidNumber(phone))
            {
                cell.ErrorMessage = "Invalid phone number";
            }
            else
            {
                cell.Value = util.Format(phone, PhoneNumberFormat.E164);
            }
        }
        catch (Exception ex)
        {
            cell.ErrorMessage = ex.Message;
        }

    }
}
