using BBWM.Core.DTO;

namespace BBWM.FormIO.DTO.FormViewDTOs;

public class FormRevisionViewDTO: IDTO
{
    public int Id { get; set; }
    public string? Json { get; set; }

    // Foreign keys and navigational properties.
    public int FormDefinitionId { get; set; }
    public FormDefinitionViewDTO FormDefinition { get; set; } = null!;
}