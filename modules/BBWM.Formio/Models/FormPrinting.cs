using BBWM.Core.Data;

namespace BBWM.FormIO.Models
{
    public class FormPrinting : IEntity
    {
        public int Id { get; set; }

        // Foreign keys and navigational properties.
        public string UserId { get; set; } = null!;

        public int FormDataId { get; set; }
        public FormDefinition? FormData { get; set; }
    }
}