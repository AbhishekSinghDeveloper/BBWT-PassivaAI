using BBWM.Core.DTO;

namespace BBWM.AggregatedLogs;

public class LogDTO : IDTO
{
    public int Id { get; set; }

    public string Message { get; set; }

    public string Level { get; set; }

    public DateTimeOffset TimeStamp { get; set; }

    public string Exception { get; set; }

    public string LogEvent { get; set; }

    public string AppName { get; set; }

    public string Server { get; set; }

    public string IP { get; set; }

    public string Source { get; set; }

    public string UserName { get; set; }

    public bool? IsImpersonating { get; set; }

    public string OriginalUserName { get; set; }

    public string ErrorId { get; set; }

    public int? HttpStatus { get; set; }
}