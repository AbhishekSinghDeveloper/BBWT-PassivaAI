using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.Validation;

using Xunit;

namespace BBWM.DataProcessing.Test.Validators;

public class EmailValidatorTests
{
    [Fact]
    public void Email_Validator()
    {
        // arrange
        var validator = new EmailValidator();

        var cell1 = new ImportEntryCell("test@mail.com", null);
        var cell2 = new ImportEntryCell("test.mail.com", null);
        var empty = new ImportEntryCell("", null);

        // act
        validator.PerformValidation(cell1);
        validator.PerformValidation(cell2);
        validator.PerformValidation(empty);

        // assert
        Assert.True(cell1.IsValid);
        Assert.False(cell2.IsValid);
        Assert.False(empty.IsValid);
    }
}
