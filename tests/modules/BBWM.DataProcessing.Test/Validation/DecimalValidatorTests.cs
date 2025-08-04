using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.Validation;

using Xunit;

namespace BBWM.DataProcessing.Test.Validation;

public class DecimalValidatorTests
{
    public DecimalValidatorTests()
    {
    }

    private static DecimalValidator GetService()
    {
        return new DecimalValidator();
    }

    [Fact]
    public void Perform_Validation_Test()
    {
        // Arrange
        var service = GetService();

        var cell1 = new ImportEntryCell("01.01.2010", null);
        var cell2 = new ImportEntryCell("20/05/2010", null);
        var cell3 = new ImportEntryCell("05/20/2010", null);
        var cell4 = new ImportEntryCell(777, null);
        var empty = new ImportEntryCell(string.Empty, null);
        var nullCell = new ImportEntryCell(null, null);

        service.PerformValidation(cell1);
        service.PerformValidation(cell2);
        service.PerformValidation(cell3);
        service.PerformValidation(cell4);
        service.PerformValidation(empty);
        service.PerformValidation(nullCell);

        Assert.False(cell1.IsValid);
        Assert.False(cell2.IsValid);
        Assert.False(cell3.IsValid);
        Assert.True(cell4.IsValid);
        Assert.False(empty.IsValid);
        Assert.False(nullCell.IsValid);
    }
}
