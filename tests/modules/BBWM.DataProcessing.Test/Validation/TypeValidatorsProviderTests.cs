using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.Validation;

using Xunit;

namespace BBWM.DataProcessing.Test.Validation;

public class TypeValidatorsProviderTests
{
    public TypeValidatorsProviderTests()
    {
    }

    private static TypeValidatorsProvider GetService()
    {
        return new TypeValidatorsProvider();
    }

    [Fact]
    public void GetValidator_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = GetService();

        var mock = new ColumnDefinition();

        mock.Type = CellDataType.Number;
        service.GetValidator(mock);

        mock.Type = CellDataType.Email;
        service.GetValidator(mock);

        mock.Type = CellDataType.Date;
        service.GetValidator(mock);

        mock.Type = CellDataType.DateTimeOffset;
        service.GetValidator(mock);

        mock.Type = CellDataType.Decimal;
        service.GetValidator(mock);

        mock.Type = CellDataType.Phone;
        service.GetValidator(mock);

        mock.Type = CellDataType.Custom;
        service.GetValidator(mock);

        mock.Type = CellDataType.String;
        service.GetValidator(mock);

        Assert.NotNull(CellDataType.String);
        Assert.NotNull(CellDataType.Number);
        Assert.NotNull(CellDataType.Email);
        Assert.NotNull(CellDataType.Decimal);
        Assert.NotNull(CellDataType.DateTimeOffset);
        Assert.NotNull(CellDataType.Phone);
        Assert.NotNull(CellDataType.Custom);
    }
}
