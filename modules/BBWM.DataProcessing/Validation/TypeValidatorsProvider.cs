using BBWM.Core.Autofac;
using BBWM.DataProcessing.Classes;

namespace BBWM.DataProcessing.Validation;

/// <summary>
/// Provides validator for the column definition's type 
/// </summary>
public class TypeValidatorsProvider : ITypeValidatorsProvider
{
    /// <summary>
    /// Returns validator for the column definition's type
    /// </summary>
    /// <param name="columnDefinition">Column definition</param>
    /// <returns>Validator instance</returns>
    [IgnoreLogging]
    public EntryCellValidator GetValidator(ColumnDefinition columnDefinition)
    {
        var typeInfo = columnDefinition.TypeInfo;
        switch (columnDefinition.Type)
        {
            case CellDataType.Number:
                return new NumberValidator();
            case CellDataType.Decimal:
                return new DecimalValidator();
            case CellDataType.Date:
                return new DateValidator(typeInfo as DateTimeCellDataTypeInfo);
            case CellDataType.DateTimeOffset:
                return new DateTimeOffsetValidator(typeInfo as DateTimeCellDataTypeInfo);
            case CellDataType.Phone:
                return new PhoneValidator();
            case CellDataType.Email:
                return new EmailValidator();
            case CellDataType.Custom:
                return new CustomValidator(typeInfo as CustomCellDataTypeInfo);
            default:
                return new StringValidator();
        }
    }
}
