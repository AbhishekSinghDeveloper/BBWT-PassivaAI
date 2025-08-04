using BBWM.DataProcessing.Classes;

using System.Globalization;

namespace BBWM.DataProcessing.Validation;

/// <summary>
/// Validates the cell for DateTime
/// </summary>
public class DateValidator : EntryCellValidator
{
    private readonly string _dateFormat = CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern;

    /// <summary>
    /// Custom constructor
    /// </summary>
    /// <param name="typeInfo">type info</param>
    public DateValidator(DateTimeCellDataTypeInfo typeInfo = null)
    {
        if (typeInfo is null) return;
        if (!string.IsNullOrWhiteSpace(typeInfo.DateFormats))
        {
            _dateFormat = $"{_dateFormat}, {typeInfo.DateFormats}";
        }
    }

    /// <summary>
    /// Custom constructor
    /// </summary>
    /// <param name="typeInfo">type info</param>
    public override void PerformValidation(ImportEntryCell cell)
    {
        DateTime dateTime;
        bool isValid;

        var formats = _dateFormat.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).Distinct().ToArray();
        isValid = DateTime.TryParseExact(Convert.ToString(cell.Value).Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);

        if (isValid)
        {
            cell.Value = dateTime;
        }
        else
        {

            cell.ErrorMessage = $"The value \"{(cell.Value ?? "null").ToString().Trim()}\" does not match the specified date formats: {string.Join(", ", formats.Select(f => $"'{f}'"))}";
        }
    }
}
