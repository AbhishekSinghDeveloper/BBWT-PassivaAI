using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.Validation;

using Xunit;

namespace BBWM.DataProcessing.Test.Validators;

public class DateValidatorTests
{
    [Fact]
    public void Date_Validator_Basic()
    {
        // arrange
        var validator = new DateValidator();

        var cell1 = new ImportEntryCell("01.01.2010", null);
        var cell2 = new ImportEntryCell("20/05/2010", null);
        var cell3 = new ImportEntryCell("05/20/2010", null);
        var empty = new ImportEntryCell(string.Empty, null);
        var nullCell = new ImportEntryCell(null, null);

        // act
        validator.PerformValidation(cell1);
        validator.PerformValidation(cell2);
        validator.PerformValidation(cell3);
        validator.PerformValidation(empty);
        validator.PerformValidation(nullCell);

        // assert
        Assert.False(cell1.IsValid);
        Assert.False(cell2.IsValid);
        Assert.True(cell3.IsValid);
        Assert.False(empty.IsValid);
        Assert.False(nullCell.IsValid);
    }

    [Fact]
    public void Date_Validator_Extended()
    {
        // arrange
        var info = new DateTimeCellDataTypeInfo("MM-dd-yyyy, dd.MM.yyyy");
        var validator = new DateValidator(info);

        var cell1 = new ImportEntryCell("01.01.2010", null);
        var cell2 = new ImportEntryCell("05-20-2010", null);
        var empty = new ImportEntryCell("", null);

        // act
        validator.PerformValidation(cell1);
        validator.PerformValidation(cell2);
        validator.PerformValidation(empty);

        // assert
        Assert.True(cell1.IsValid);
        Assert.True(cell2.IsValid);
        Assert.False(empty.IsValid);
    }
}
