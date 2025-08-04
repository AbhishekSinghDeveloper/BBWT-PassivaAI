using System.Runtime.Serialization;

namespace BBF.Reporting.QueryBuilder.Enums;

public enum ComparisonOperation
{
    [EnumMember(Value = "=")] Equals,
    [EnumMember(Value = "<")] LessThan,
    [EnumMember(Value = "!=")] NotEqual,
    [EnumMember(Value = ">")] GreaterThan,
    [EnumMember(Value = ">=")] GreaterThanOrEqual,
    [EnumMember(Value = "<>")] LessOrGreaterThan,
    [EnumMember(Value = "<=")] LessThanOrEqual,
}