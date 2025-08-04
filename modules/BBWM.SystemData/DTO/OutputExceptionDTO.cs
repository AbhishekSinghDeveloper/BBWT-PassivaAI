namespace BBWM.SystemData.DTO;

public class OutputExceptionDTO
{
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public string Source { get; set; }
    public string InnerExMessage { get; set; }
    public string InnerExStackTrace { get; set; }
    public string InnerExSource { get; set; }
}
