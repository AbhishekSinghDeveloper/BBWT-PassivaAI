namespace BBWM.AWS.EventBridge.Test.AwsCron;

public class AwsCronExpressionTestsData
{
    private static DateTime Now => DateTime.UtcNow;


    public static DateTime BaseDateTime => new(2020, 5, 9, 22, 30, 0, DateTimeKind.Utc);

    public static IEnumerable<object[]> NextShouldBeTestData => new[]
    {
            // AWS-cron expression                                          Next occurrence
            new[] { "cron(1,24,50-55,58 * 25 MAR/4 ? 2020,2021,2023,2028)", "Sat, 25 Jul 2020 00:01:00 GMT" },
            new[] { "cron(9 * 7,9,11 3,5,7 ? 2021)",                        "Sun, 07 Mar 2021 00:09:00 GMT" },
            new[] { "cron(9 * 7,9,11 5 ? 2021)",                            "Fri, 07 May 2021 00:09:00 GMT" },
            new[] { "cron(9 * 7,9,11 5 ? 2020)",                            "Sat, 09 May 2020 23:09:00 GMT" },
            new[] { "cron(9 8-20 7,9,11 5 ? 2020)",                         "Mon, 11 May 2020 08:09:00 GMT" },
            new[] { "cron(9 8-20 ? 5 MON-WED,FRI 2020)",                    "Mon, 11 May 2020 08:09:00 GMT" },
            new[] { "cron(9 8-20 ? 5 2-4,6 2020)",                          "Mon, 11 May 2020 08:09:00 GMT" },
            new[] { "cron(3 2-5 ? 4,6,8,10 TUE,THU,SAT 2020)",              "Tue, 02 Jun 2020 02:03:00 GMT" },
            new[] { "cron(9 * L 5 ? 2019,2020)",                            "Sun, 31 May 2020 00:09:00 GMT" },
            new[] { "cron(19 4 L 9 ? 2019,2020)",                           "Wed, 30 Sep 2020 04:19:00 GMT" },
            new[] { "cron(19 4 3W 9 ? 2019,2020)",                          "Thu, 03 Sep 2020 04:19:00 GMT" },
            new[] { "cron(19 4 5W 9 ? 2019,2020)",                          "Fri, 04 Sep 2020 04:19:00 GMT" },
            new[] { "cron(9 8-20 ? 8 5#2 2019,2020)",                       "Thu, 13 Aug 2020 08:09:00 GMT" },
            new[] { "cron(9 8-20 ? 8 5#3 2019,2020)",                       "Thu, 20 Aug 2020 08:09:00 GMT" },
            new[] { "cron(9 8-20 ? 12 3#1 2019,2020)",                      "Tue, 01 Dec 2020 08:09:00 GMT" },
            new[] { "cron(9 8-20 ? 12 3#5 2019,2020)",                      "Tue, 29 Dec 2020 08:09:00 GMT" },
            new[] { "cron(0 0 1 1 ? *)",                                    "Fri, 01 Jan 2021 00:00:00 GMT" },
            new[] { "cron(0 1 2 3 ? *)",                                    "Tue, 02 Mar 2021 01:00:00 GMT" },
            new[] { "cron(7 1 2 3 ? *)",                                    "Tue, 02 Mar 2021 01:07:00 GMT" },
            new[] { "cron(* * 2 3 ? *)",                                    "Tue, 02 Mar 2021 00:00:00 GMT" },
            new[] { "cron(* * * * ? 2300)",                                 "Mon, 01 Jan 2300 00:00:00 GMT" },
            new[] { "cron(* * * * ? 2300-2302)",                            "Mon, 01 Jan 2300 00:00:00 GMT" },
            new[] { "cron(* * * * ? 2300/10)",                              "Mon, 01 Jan 2300 00:00:00 GMT" },
            new[] { "cron(* * * * ? */1)",                                  $"{BaseDateTime:R}" },
        };

    public static IEnumerable<object[]> NextShouldBeNullTestData => new[]
    {
            // Invalid AWS-cron expression
            new[] { "cron(* * * * ? 2018)" },   // Produces no value
            new[] { "cron(* * * * ? x)" },      // Invalid year
            new[] { "cron(* * * * ? )" },       // Empty year
            new[] { "cron(* * * * ? */)" },     // Invalid year increment
            new[] { "cron(* * * ? */2)" },      // Missing field
            new[] { "cron(* * * * ?" },         // Incomplete
            new[] { "blah" },                   // Anything
            new[] { string.Empty },
            new[] { (string)null },
        };
}
