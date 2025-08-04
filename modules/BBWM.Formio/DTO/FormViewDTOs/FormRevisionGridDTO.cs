using BBWM.Core.DTO;

namespace BBWM.FormIO.DTO.FormViewDTOs;

public class FormRevisionGridDTO : IDTO
{
    public int Id { get; set; }
    public string Json { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string ViewName { get; set; } = null!;

    // Foreign keys and navigational properties.
    public int FormDefinitionId { get; set; }
    public FormDefinitionDTO? FormDefinition { get; set; }

    public int? ParentFormRevisionGridId { get; set; }
    public FormRevisionGridDTO? ParentFormRevisionGrid { get; set; }
}