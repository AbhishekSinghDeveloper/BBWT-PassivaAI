using BBWM.Core.DTO;

using System.ComponentModel.DataAnnotations;

namespace BBWM.Messages.Templates;

public class EmailTemplateDTO : IDTO
{
    public int Id { get; set; }
    [Required]
    public string Code { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public string From { get; set; }
    [Required]
    public string Subject { get; set; }
    [Required]
    public string Body { get; set; }
    public bool IsSystem { get; set; }
}
