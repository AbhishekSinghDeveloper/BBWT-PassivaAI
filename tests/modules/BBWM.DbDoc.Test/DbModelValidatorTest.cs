using BBWM.DbDoc.Core.Classes.ValidationRules;
using BBWM.DbDoc.Enums;
using BBWM.DbDoc.Services;

using Xunit;

namespace BBWT.Tests.modules.BBWM.DbDoc.Test;

public class DbModelValidatorTest
{
    [Fact]
    public async Task Required_Test()
    {
        var validator = new DbModelValidator();
        var rule = new RequiredValidationRule();

        Assert.False(validator.Validate(rule, null));
        Assert.True(validator.Validate(rule, 0));
        Assert.True(validator.Validate(rule, string.Empty));
        Assert.True(validator.Validate(rule, DateTime.Now));
    }

    [Fact]
    public async Task NumberRange_Test()
    {
        var validator = new DbModelValidator();
        var min = 10;
        var max = 20;
        var rule = new NumberRangeValidationRule
        {
            Min = min,
            Max = max
        };

        Assert.False(validator.Validate(rule, 9));
        Assert.True(validator.Validate(rule, 10));
        Assert.True(validator.Validate(rule, 15));
        Assert.True(validator.Validate(rule, 20));
        Assert.False(validator.Validate(rule, 21));

        rule.Min = null;

        Assert.True(validator.Validate(rule, 9));
        Assert.True(validator.Validate(rule, 15));
        Assert.False(validator.Validate(rule, 21));

        rule.Min = min;
        rule.Max = null;

        Assert.False(validator.Validate(rule, 9));
        Assert.True(validator.Validate(rule, 15));
        Assert.True(validator.Validate(rule, 21));
    }

    [Fact]
    public async Task DateRange_Test()
    {
        var validator = new DbModelValidator();
        var min = new DateTime(1000, 1, 1);
        var max = new DateTime(2000, 1, 1);
        var rule = new DateRangeValidationRule
        {
            Min = min,
            Max = max
        };

        Assert.False(validator.Validate(rule, new DateTime(900, 1, 1)));
        Assert.True(validator.Validate(rule, new DateTime(1000, 1, 1)));
        Assert.True(validator.Validate(rule, new DateTime(1500, 1, 1)));
        Assert.True(validator.Validate(rule, new DateTime(2000, 1, 1)));
        Assert.False(validator.Validate(rule, new DateTime(2100, 1, 1)));

        rule.Min = null;

        Assert.True(validator.Validate(rule, new DateTime(900, 1, 1)));
        Assert.True(validator.Validate(rule, new DateTime(1500, 1, 1)));
        Assert.False(validator.Validate(rule, new DateTime(2100, 1, 1)));

        rule.Min = min;
        rule.Max = null;

        Assert.False(validator.Validate(rule, new DateTime(900, 1, 1)));
        Assert.True(validator.Validate(rule, new DateTime(1500, 1, 1)));
        Assert.True(validator.Validate(rule, new DateTime(2100, 1, 1)));
    }

    [Fact]
    public async Task InputFormat_Test()
    {
        var validator = new DbModelValidator();
        var rule = new InputFormatValidationRule
        {
            Type = InputFormat.Phone
        };

        Assert.False(validator.Validate(rule, string.Empty));
        Assert.True(validator.Validate(rule, "+41 22 749 01 11"));

        rule.Type = InputFormat.Email;

        Assert.False(validator.Validate(rule, string.Empty));
        Assert.True(validator.Validate(rule, "test@test.ts"));

        rule.Type = InputFormat.Url;

        Assert.False(validator.Validate(rule, string.Empty));
        Assert.True(validator.Validate(rule, "http://some-url.test"));

        rule.Type = InputFormat.Regex;
        rule.Format = "\\d{1}";

        Assert.False(validator.Validate(rule, "a"));
        Assert.True(validator.Validate(rule, "1"));
    }

    [Fact]
    public async Task MaxLength_Test()
    {
        var validator = new DbModelValidator();
        var rule = new MaxLengthValidationRule
        {
            MaxLength = 10
        };

        Assert.False(validator.Validate(rule, "01234567899"));
        Assert.True(validator.Validate(rule, "0123456789"));
    }
}
