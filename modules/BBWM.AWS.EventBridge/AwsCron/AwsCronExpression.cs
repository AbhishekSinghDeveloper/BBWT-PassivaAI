using Cronos;

using System.Text.RegularExpressions;

namespace BBWM.AWS.EventBridge.AwsCron;

public sealed class AwsCronExpression
{
    private static readonly Regex cronRe = new Regex(@"cron\(((?:[^\s]+\s+){4})([^\s]+)\s+([^\s]+)\)");

    private readonly CronExpression cronExpression;
    private readonly YearField yearField;

    private AwsCronExpression()
        : this(default, default)
    { }

    private AwsCronExpression(
        CronExpression cronExpression,
        YearField yearField)
    {
        this.cronExpression = cronExpression;
        this.yearField = yearField;
    }

    public static AwsCronExpression Parse(string awsCron)
    {
        var m = cronRe.Match(awsCron ?? "");
        if (m.Success)
        {
            try
            {
                var start = m.Groups[1].Value.Trim();
                var dayOfWeek = DayOfWeekField.FixForCronos(m.Groups[2].Value.Trim());
                var year = m.Groups[3].Value.Trim();

                var cronExpr = CronExpression.Parse($"{start} {dayOfWeek}", CronFormat.Standard);

                return new AwsCronExpression(cronExpr, YearField.Parse(year));
            }
            catch
            {
                /*
                 * Always create an AwsCronExpression instance even if 
                 * there's a problem with the input aws-cron expression
                 */
            }
        }

        return new AwsCronExpression();
    }

    public DateTime? GetNextOccurrence(DateTime utcDate)
    {
        try
        {
            if (yearField.Any)
            { return cronExpression.GetNextOccurrence(utcDate, inclusive: true); }


            var toUtc = new DateTime(yearField.MaxYear + 1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var fromUtc = new DateTime(yearField.MinYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            if (utcDate > fromUtc)
            { fromUtc = utcDate; }

            var nextOccurrence = cronExpression
                .GetOccurrences(fromUtc, toUtc)
                .FirstOrDefault(occ => yearField.HasYear(occ.Year));

            if (nextOccurrence >= utcDate && nextOccurrence < toUtc)
            { return nextOccurrence; }
        }
        catch
        { /* Capture and give no occurrence to the caller */ }

        return default;
    }
}
