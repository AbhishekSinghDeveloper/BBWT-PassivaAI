using System.Runtime.Serialization;

namespace BBF.Reporting.QueryBuilder.Enums;

public enum QueryBinaryOperation
{
    [EnumMember(Value = "union")] Union,
    [EnumMember(Value = "unionAll")] UnionAll,
    [EnumMember(Value = "intersect")] Intersect,
    [EnumMember(Value = "except")] Except,
}