namespace BBWM.AWS.EventBridge.AwsCron;

internal static class ArrayExtensions
{
    public static void Deconstruct<T>(this T[] source, out T t0, out T t1)
    {
        t0 = default;
        t1 = default;

        if (source is not null)
        {
            if (source.Length > 0)
            { t0 = source[0]; }
            if (source.Length > 1)
            { t1 = source[1]; }
        }
    }
}
