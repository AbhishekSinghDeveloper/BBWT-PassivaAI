using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.Validation;

using Xunit;

namespace BBWM.DataProcessing.Test.Validators;

public class NumberValidatorTests
{
    [Fact]
    public void Number_Validator()
    {
        // arrange
        var validator = new NumberValidator();

        var cell1 = new ImportEntryCell("10", null);
        var cell2 = new ImportEntryCell("41.454", null);
        var cell3 = new ImportEntryCell("test", null);
        var empty = new ImportEntryCell("", null);

        // act
        validator.PerformValidation(cell1);
        validator.PerformValidation(cell2);
        validator.PerformValidation(cell3);
        validator.PerformValidation(empty);

        // assert
        Assert.True(cell1.IsValid);
        Assert.True(cell2.IsValid);
        Assert.False(cell3.IsValid);
        Assert.False(empty.IsValid);
    }

    [Fact]
    public void Number_Validator2()
    {
        // arrange
        var validator = new NumberValidator(new NumberCellDataTypeInfo(3, 8));

        var cell1 = new ImportEntryCell("9", null);
        var cell2 = new ImportEntryCell("2", null);
        var cell3 = new ImportEntryCell("6", null);

        // act
        validator.PerformValidation(cell1);
        validator.PerformValidation(cell2);
        validator.PerformValidation(cell3);

        // assert
        Assert.False(cell1.IsValid);
        Assert.False(cell2.IsValid);
        Assert.True(cell3.IsValid);
    }

    [Fact]
    public void Number_Validator3()
    {
        // arrange
        var validator = new NumberValidator(new NumberCellDataTypeInfo(8, 3));

        var cell1 = new ImportEntryCell("9", null);

        // act
        validator.PerformValidation(cell1);

        // assert
        Assert.False(cell1.IsValid);
    }
}
