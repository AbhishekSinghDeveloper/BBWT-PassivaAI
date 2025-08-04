using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.Validation;

using Moq;

using Xunit;

namespace BBWM.DataProcessing.Test.Validation;

public class DateTimeOffsetValidatorTests
{
    public DateTimeOffsetValidatorTests()
    {
    }

    private static DateTimeOffsetValidator GetService()
    {
        var mock = new DateTimeCellDataTypeInfo();

        var customValidationHandler = new Mock<CustomValidationHandler>();
        var customCellDataTypeInfo = new CustomCellDataTypeInfo(customValidationHandler.Object);

        DateTime dt = DateTime.Now; // Or whatever
        string s = dt.ToString("yyyyMMddHHmmss");

        mock.DateFormats = s;

        return new DateTimeOffsetValidator(mock);
    }

    [Fact]
    public void Perform_Validation_Test()
    {
        // Arrange
        var service = GetService();

        var cell1 = new ImportEntryCell("01.01.2010", null);
        var cell2 = new ImportEntryCell("20/05/2010", null);
        var cell3 = new ImportEntryCell("05/20/2010", null);
        var empty = new ImportEntryCell(string.Empty, null);
        var nullCell = new ImportEntryCell(null, null);

        service.PerformValidation(cell1);
        service.PerformValidation(cell2);
        service.PerformValidation(cell3);
        service.PerformValidation(empty);
        //service.PerformValidation(nullCell);

        Action result = () => service.PerformValidation(cell1);

        Action errorResult = () => service.PerformValidation(nullCell);

        Assert.NotNull(result);
        Assert.Throws<NullReferenceException>(errorResult);
    }
}
