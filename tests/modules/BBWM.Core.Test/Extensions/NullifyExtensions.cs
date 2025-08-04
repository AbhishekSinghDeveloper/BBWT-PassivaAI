using System.Text.Json;

namespace BBWT.Tests.modules.BBWM.Core.Test.Extensions;

public static class NullifyExtensions
{
    public static T Nullify<T>(this T @object, Action<T> setNulls)
        where T : class
    {
        if (@object is null)
            return null;

        var json = JsonSerializer.Serialize(@object);
        var newObject = JsonSerializer.Deserialize<T>(json);

        setNulls?.Invoke(newObject);
        return newObject;
    }
}
