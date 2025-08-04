using BBWM.Core.DTO;

namespace BBWM.FormIO.DTO
{
    public class FormDefinitionDTO : IDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ManagerId { get; set; }
        public string? ViewName { get; set; }
        public bool IsPublished { get; set; }
        public bool? ByRequestOnly { get; set; }

        // Foreign keys and navigational properties.
        public List<int>? OrganizationIds { get; set; }

        public int? FormCategoryId { get; set; }
        public virtual FormCategoryDTO? FormCategory { get; set; }

        public int ActiveRevisionId { get; set; }
        public FormRevisionMinDTO ActiveRevision { get; set; } = null!;
    }

    // DTO for new form definition request creation.
    public class FormDefinitionForNewRequestDTO
    {
        public required string Name { get; set; }
        public string? ManagerId { get; set; }
        public bool? ByRequestOnly { get; set; }
        public int? FormCategoryId { get; set; }
        public required InitialFormRevisionRequestDTO FormRevisionData { get; set; }
    }

    public class PublishFormDefinitionDTO
    {
        public int FormId { get; set; }
        public int FormCat { get; set; }
        public List<int> OrgIds { get; set; } = new();
    }

    public class FormDefinitionPageDTO : FormDefinitionDTO
    {
        public string Creator { get; set; } = null!;
        public string Org { get; set; } = null!;
        public string Category { get; set; } = null!;
        public int FormDataCount { get; set; }
    }

    public class ChangeFormDefinitionOwnerDTO
    {
        public string NewOwnerId { get; set; } = null!;
        public int FormDefinitionId { get; set; }
    }

    public class FormDefinitionComposedDTO : FormDefinitionDTO
    {
        public string Json { get; set; } = null!;
        public bool MobileFriendly { get; set; } = false;
        public List<FormJsonSelectDBField> Fields { get; set; } = new();
    }

    public class FormJsonSelectDBField
    {
        public string FieldKey { get; set; } = null!;
        public bool MultiValue { get; set; }
        public List<FormJsonSelectDBFieldValue> Values { get; set; } = new();
    }

    public class FormJsonSelectDBFieldValue
    {
        public string Label { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class FormDefinitionParameters
    {
        public List<string> ParameterString { get; set; } = new();
    }

    public class FormDefinitionForRequestMinDTO : IDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class FormDefinitionForFormDataPageDTO : FormDefinitionForRequestMinDTO
    {
    }
}