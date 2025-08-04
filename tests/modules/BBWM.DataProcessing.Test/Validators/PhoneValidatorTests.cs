using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.Validation;

using Xunit;

namespace BBWM.DataProcessing.Test.Validators;

public class PhoneValidatorTests
{
    [Fact]
    public void Phone_Validator()
    {
        // arrange
        var validator = new PhoneValidator();

        var cell1 = new ImportEntryCell("+79024852682", null);
        var cell2 = new ImportEntryCell("9024852682", null);
        var cell3 = new ImportEntryCell("test", null);
        var cell4 = new ImportEntryCell("+790248526824848", null);
        var empty = new ImportEntryCell("", null);

        // act
        validator.PerformValidation(cell1);
        validator.PerformValidation(cell2);
        validator.PerformValidation(cell3);
        validator.PerformValidation(cell4);
        validator.PerformValidation(empty);

        // assert
        Assert.True(cell1.IsValid);
        Assert.True(cell2.IsValid);
        Assert.False(cell3.IsValid);
        Assert.False(cell4.IsValid);
        Assert.False(empty.IsValid);
    }
}
