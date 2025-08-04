namespace BBWM.FormIO.DTO
{
    public class FormSurveyDataDTO : FormDataDTO
    {
        public string RespondentId { get; set; } = null!;
        public string RespondentFullName { get; set; } = null!;
        public string RespondentUserName { get; set; } = null!;
        public string FormRevisionJson { get; set; } = null!;
    }
}