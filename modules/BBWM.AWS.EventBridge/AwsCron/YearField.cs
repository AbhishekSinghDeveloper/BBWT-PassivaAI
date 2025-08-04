namespace BBWM.AWS.EventBridge.AwsCron;

internal class YearField
{
    public const int MIN_YEAR = 1970;
    public const int MAX_YEAR = 2199;
    private readonly HashSet<int> values;
    private int? maxYear = null;
    private int? minYear = null;

    private YearField()
        : this(true)
    { }

    private YearField(bool any, HashSet<int> values = null)
    {
        Any = any;
        this.values = values ?? new HashSet<int>();
    }

    public bool Any { get; }

    public bool HasYear(int year) => Any ? true : values.Contains(year);

    public int MaxYear => maxYear ??= values.Max();

    public int MinYear => minYear ??= values.Min();

    public static YearField MatchAny => new YearField();

    public static YearField Parse(string field)
    {
        if (string.IsNullOrEmpty(field))
        { throw new ArgumentNullException(nameof(field)); }

        if (field == "*")
        { return MatchAny; }

        var years = new HashSet<int>();
        foreach (var part in field.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part.IndexOf('-') >= 0)
            { ParseRange(part.Split('-'), years); }
            else if (part.IndexOf('/') >= 0)
            { ParseIncrement(part.Split('/'), years); }
            else
            { years.Add(ParseValue(part)); }
        }

        return new YearField(false, years);
    }

    private static int ParseValue(string part)
    {
        int.TryParse(part, out var year);
        return year;
    }

    private static void ParseIncrement(string[] parts, HashSet<int> years)
    {
        var (yearStr, incrementStr) = parts;
        if (yearStr == "*")
        { yearStr = $"{MIN_YEAR}"; }
        var year = ParseValue(yearStr);
        if (!int.TryParse(incrementStr, out var increment))
        { throw new ArgumentException(nameof(increment)); }

        Increment(year, increment, MAX_YEAR, years);

    }

    private static void ParseRange(string[] range, HashSet<int> years)
    {
        var (yearStartStr, yearEndStr) = range;
        var yearStart = ParseValue(yearStartStr);
        var yearEnd = ParseValue(yearEndStr);

        Increment(yearStart, 1, yearEnd, years);
    }

    private static void Increment(int year, int increment, int max, HashSet<int> years)
    {
        do
        {
            years.Add(year);
            year += increment;
        } while (year <= max);
    }
}
