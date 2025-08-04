namespace BBWM.AWS.EventBridge.AwsCron;

internal class DayOfWeekField
{
    private DayOfWeekField() { }

    public static string FixForCronos(string cronosCron)
    {
        int hashIndex;
        if ((hashIndex = cronosCron.IndexOf('#')) >= 0)
        {
            var d = short.Parse($"{cronosCron[hashIndex - 1]}") - 1;
            return cronosCron.Insert(hashIndex - 1, $"{d}").Remove(hashIndex, 1);
        }

        var fixedParts = new List<string>();

        foreach (var part in cronosCron.Split(","))
        {
            if (short.TryParse(part, out var value))
            {
                fixedParts.Add($"{value - 1}"); continue;
            }
            else if (part.IndexOf('-') >= 0)
            {
                var (_s, _e) = part.Split('-');
                if (short.TryParse(_s, out var s) && short.TryParse(_e, out var e))
                { fixedParts.Add($"{s - 1}-{e - 1}"); continue; }
            }
            else
            {
                // do nothing
            }


            fixedParts.Add(part);
        }

        return string.Join(',', fixedParts);
    }
}
