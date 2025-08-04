using BBWM.Core.DTO;

namespace BBWM.FormIO.DTO.FormViewDTOs;

public class FormDefinitionViewDTO: IDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? ViewName { get; set; }

    // Foreign key and navigational properties.
    public int ActiveRevisionId { get; set; }
    public FormRevisionViewDTO? ActiveRevision { get; set; }
    public ICollection<FormRevisionGridDTO> FormRevisionGrids { get; set; } = new List<FormRevisionGridDTO>();
}