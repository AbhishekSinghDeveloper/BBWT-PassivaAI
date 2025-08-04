using BBWM.Core.DTO;

namespace BBWM.FormIO.DTO
{
    public class FormDataDTO : IDTO
    {
        public int Id { get; set; }
        public string Json { get; set; } = null!;
        public DateTimeOffset CreatedOn { get; set; }

        public bool IsMUF { get; set; }
        public int MultiUserFormAssocLinkId { get; set; }
        public int MufAssocId { get; set; }

        // Foreign keys and navigational properties.
        public int? DraftId { get; set; }
        public int? RequestId { get; set; }
        public int? OrganizationId { get; set; }
        public string UserId { get; set; } = null!;

        public int? FormDefinitionId { get; set; }
        public FormDefinitionDTO? FormDefinition { get; set; }

        public int? SurveyId { get; set; }
        public FormSurveyDTO? Survey { get; set; }
    }

    public class FormDataPageDTO : IDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Version { get; set; } = null!;
        public DateTimeOffset CreatedOn { get; set; }

        // Foreign keys and navigational properties.
        public int FormDefinitionId { get; set; }
        public FormDefinitionDTO FormDefinition { get; set; } = null!;

        public int? SurveyId { get; set; }
        public FormSurveyMinDTO? Survey { get; set; }
    }
}