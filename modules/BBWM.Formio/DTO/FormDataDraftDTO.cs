using BBWM.Core.DTO;

namespace BBWM.FormIO.DTO
{
    public class FormDataDraftDTO : IDTO
    {
        public int Id { get; set; }
        public string Json { get; set; } = null!;
        public DateTimeOffset CreatedOn { get; set; }

        // Foreign keys and navigational properties.
        public int FormRevisionId { get; set; }
        public string UserId { get; set; } = null!;
    }
}