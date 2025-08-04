using BBWM.Core.DTO;

namespace BBWM.FormIO.DTO
{
    public class FormSurveyDTO : IDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTimeOffset Created { get; set; }

        public int FormRevisionId { get; set; }
        public List<string> SurveyedUsers { get; set; } = new();
        public ICollection<FormSurveyDataDTO> SurveyFormDataInstances { get; set; } = new List<FormSurveyDataDTO>();
    }

    public class FormSurveyPageDTO : FormSurveyDTO
    {
        public string? FormDefinitionName { get; set; }
        public string? Version { get; set; }
        public string? Orgs { get; set; }
    }

    public class FormSurveyMinDTO : IDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}