using BBWM.Core.DTO;
using BBWM.FormIO.Models;

namespace BBWM.FormIO.DTO
{
    public class FormRevisionDTO : IDTO
    {
        public int Id { get; set; }
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public bool MobileFriendly { get; set; } = false;
        public string? Note { get; set; }
        public string Json { get; set; } = null!;
        public bool? MUFCapable { get; set; }

        // Foreign keys and navigational properties.
        public string? CreatorId { get; set; }
        public string? CreatorName { get; set; }

        public int FormDefinitionId { get; set; }
        public FormDefinitionDTO? FormDefinition { get; set; }

        public ICollection<FormData> FormData { get; set; } = new List<FormData>();
    }

    public class NewFormRevisionRequestDTO
    {
        public bool MobileFriendly { get; set; } = false;
        public string? Note { get; set; }
        public required string Json { get; set; }
        public bool? MUFCapable { get; set; }

        // Foreign keys and navigational properties.
        public string? CreatorId { get; set; }
        public int FormDefinitionId { get; set; }
    }

    public class UpdateFormRevisionRequestDTO
    {
        public bool MobileFriendly { get; set; } = false;
        public string? Note { get; set; }

        public bool IncreaseMinorVersion { get; set; }
        public bool SaveAsMajorVersion { get; set; } = false;

        public required string Json { get; set; }
        public bool? MUFCapable { get; set; }

        // Foreign keys and navigational properties.
        public string? CreatorId { get; set; }
        public required string FormDefinitionName { get; set; }
    }

    public class InitialFormRevisionRequestDTO
    {
        public bool MobileFriendly { get; set; } = false;
        public string Json { get; set; } = null!;
        public bool? MUFCapable { get; set; }
    }

    public class FormRevisionMinDTO : IDTO
    {
        public int Id { get; set; }
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string? Json { get; set; }
        public bool MobileFriendly { get; set; }
        public bool? MUFCapable { get; set; }
    }

    public class FormRevisionForRequestMinDTO : IDTO
    {
        public int Id { get; set; }

        // Foreign keys and navigational properties.
        public string? CreatorName { get; set; }

        public int FormDefinitionId { get; set; }
        public FormDefinitionForRequestMinDTO? FormDefinition { get; set; }
    }

    public class FormRevisionForFormDataPageDTO : FormRevisionForRequestMinDTO
    {
    }
}