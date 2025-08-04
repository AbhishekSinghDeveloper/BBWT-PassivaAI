using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.Validation;

using Moq;

using Xunit;

namespace BBWM.DataProcessing.Test.Validation;

public class CustomValidatorTests
{
    public CustomValidatorTests()
    {
    }

    private static CustomValidator GetService()
    {
        var mock = new Mock<CustomCellDataTypeInfo>();

        return new CustomValidator(mock.Object);
    }

    [Fact]
    public void Perform_Validation_Test()
    {
        // Arrange
        var service = GetService();
        service.PerformValidation(new ImportEntryCell(new object(), new ColumnDefinition()));

        Action result = () => service.PerformValidation(new ImportEntryCell(new object(), new ColumnDefinition()));

        Assert.NotNull(result);
    }
}
