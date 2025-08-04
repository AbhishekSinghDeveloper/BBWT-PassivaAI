using BBWM.Core.DTO;

namespace BBWM.FormIO.DTO
{
    public class FormRevisionSuggestionDTO : IDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}