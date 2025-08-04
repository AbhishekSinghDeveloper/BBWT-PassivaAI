using System.ComponentModel.DataAnnotations;

namespace BBWM.ReportProblem;

public class ReportProblemDTO
{
    [Required]
    public string[] ErrorLog { get; set; }
    [Required]
    public string User { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Time { get; set; }
    [Required]
    public string Subject { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public string Severity { get; set; }
}
