namespace BBWM.Demo.Culture;

using System;

/// <summary>
/// TestTimezoneDTO class
/// </summary>
public class TestTimezoneDTO
{
    /// <summary>
    /// Date from client
    /// </summary>
    public DateTimeOffset ClientDate { get; set; }

    /// <summary>
    /// Date from server
    /// </summary>
    public DateTimeOffset ServerDate { get; set; }
}
