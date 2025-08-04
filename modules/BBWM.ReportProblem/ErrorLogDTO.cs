using System.ComponentModel.DataAnnotations;

namespace BBWM.ReportProblem;

public class ErrorLogDTO
{
    [Required]
    public string ExceptionType { get; set; }
    [Required]
    public string ExceptionMessage { get; set; }
    [Required]
    public string StackTrace { get; set; }
    [Required]
    public string PathBase { get; set; }
    [Required]
    public string Path { get; set; }

}
