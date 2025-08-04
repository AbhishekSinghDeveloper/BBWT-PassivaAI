using BBWM.DbDoc.Core.Classes.ValidationRules;
using BBWM.DbDoc.Enums;
using BBWM.DbDoc.Model;
using BBWM.DbDoc.Services;

using Bogus;

using Moq;

using Xunit;

namespace BBWM.DataProcessing.Test.Model.ValidationMetadata;

public class DateRangeValidationRuleTests
{
    public DateRangeValidationRuleTests()
    {
    }

    private static DateRangeValidationRule GetService()
    {
        return new DateRangeValidationRule();
    }

    [Fact]
    public void Accept_Validator_Test()
    {
        // Arrange
        var service = GetService();
        var service2 = new InputFormatValidationRule();
        var service3 = new MaxLengthValidationRule { MaxLength = 1 };
        var service4 = new NumberRangeValidationRule();
        var service5 = new RequiredValidationRule();

        var dbColumnType = new Faker<ColumnType>();
        dbColumnType.RuleFor(p => p.Id, s => Guid.NewGuid());
        dbColumnType.RuleFor(p => p.Name, s => s.Random.AlphaNumeric(7));
        dbColumnType.RuleFor(p => p.Group, s => new ClrTypeGroup());
        dbColumnType.RuleFor(p => p.AnonymizationRule, s => AnonymizationRule.Date);
        dbColumnType.RuleFor(p => p.ViewMetadata, s => new ColumnViewMetadata());
        dbColumnType.RuleFor(p => p.ValidationMetadata, s => new ColumnValidationMetadata());
        dbColumnType.Generate();

        var dbColumnViewMetadata = new Faker<ColumnViewMetadata>();
        dbColumnViewMetadata.RuleFor(p => p.GridColumnView, s => new GridColumnView());
        dbColumnViewMetadata.Generate();

        FakeClass dateTime = new FakeClass()
        {
            date = new DateTime(),
        };

        var validator = new DbModelValidator();

        Action resultTrue = () => service.AcceptValidator(validator, dateTime.date);
        service.AcceptValidator(validator, dateTime.date);
        Action resultFalse = () => service.AcceptValidator(validator, new object());
        service.AcceptValidator(validator, new object());

        service2.AcceptValidator(validator, dateTime.date);
        Action resultTrue2 = () => service2.AcceptValidator(validator, dateTime.date);
        service2.AcceptValidator(validator, new object());
        Action resultFalse2 = () => service2.AcceptValidator(validator, It.IsAny<object>());
        service2.Type = InputFormat.Phone;
        service2.Format = "testFormat";

        service3.AcceptValidator(validator, It.IsAny<object>());
        Action result1 = () => service2.AcceptValidator(validator, It.IsAny<object>());
        service4.AcceptValidator(validator, It.IsAny<object>());
        Action result2 = () => service2.AcceptValidator(validator, It.IsAny<object>());
        service5.AcceptValidator(validator, It.IsAny<object>());
        Action result3 = () => service2.AcceptValidator(validator, It.IsAny<object>());

        Assert.NotNull(resultTrue);
        Assert.NotNull(resultFalse);
        Assert.NotNull(resultFalse2);
        Assert.NotNull(resultTrue2);
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
    }

    private class FakeClass
    {
        public DateTime date { get; set; }
    }
}
