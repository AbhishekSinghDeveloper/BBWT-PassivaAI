using BBWM.Core.DTO;

namespace BBWM.Messages.Templates;

public class EmailTemplateParameterDTO : IDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Notes { get; set; }
}
