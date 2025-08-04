using BBWM.DbDoc.Enums;

namespace BBWM.DbDoc.Extensions;

public static class TypeExtensions
{
    public static ClrTypeGroup GetTypeGroup(this Type type) => type switch
    {
        var t when t == typeof(string) => ClrTypeGroup.String,
        var t when t == typeof(bool) => ClrTypeGroup.Bool,
        var t when IsNumeric(t) => ClrTypeGroup.Numeric,
        var t when IsDate(t) => ClrTypeGroup.Date,
        _ => ClrTypeGroup.Other,
    };

    private static bool IsDate(Type t) =>
        t == typeof(DateTimeOffset) || t == typeof(DateTimeOffset?) ||
        t == typeof(DateTime) || t == typeof(DateTime?) ||
        t == typeof(TimeSpan) || t == typeof(TimeSpan?);

    private static bool IsNumeric(Type t) =>
        t == typeof(short) || t == typeof(short?) ||
        t == typeof(int) || t == typeof(int?) ||
        t == typeof(long) || t == typeof(long?) ||
        t == typeof(decimal) || t == typeof(decimal?) ||
        t == typeof(float) || t == typeof(float?) ||
        t == typeof(double) || t == typeof(double?);
}
