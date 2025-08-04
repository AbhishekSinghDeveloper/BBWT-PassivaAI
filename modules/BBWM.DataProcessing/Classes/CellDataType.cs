namespace BBWM.DataProcessing.Classes;

/// <summary>
/// The type of data that is used when importing data from csv/excel files
/// </summary>
public enum CellDataType
{
    /// <summary>
    /// Simple string
    /// </summary>
    String = 0,

    /// <summary>
    /// Any number (integer, float)
    /// </summary>
    Number = 1,

    /// <summary>
    /// DateTime
    /// </summary>
    Date = 2,

    /// <summary>
    /// Phone number
    /// </summary>
    Phone = 3,

    /// <summary>
    /// Email address
    /// </summary>
    Email = 4,

    /// <summary>
    /// Custom type
    /// </summary>
    Custom = 5,

    DateTimeOffset = 6,

    Decimal = 7
}
