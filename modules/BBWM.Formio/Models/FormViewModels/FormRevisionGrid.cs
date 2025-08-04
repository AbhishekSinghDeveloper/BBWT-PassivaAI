using BBWM.Core.Data;

namespace BBWM.FormIO.Models.FormViewModels;

public class FormRevisionGrid : IEntity
{
    public int Id { get; set; }
    public string Json { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string ViewName { get; set; } = null!;

    // Foreign keys and navigational properties.
    public int FormDefinitionId { get; set; }
    public FormDefinition FormDefinition { get; set; } = null!;

    public int? ParentFormRevisionGridId { get; set; }
    public FormRevisionGrid? ParentFormRevisionGrid { get; set; }
}