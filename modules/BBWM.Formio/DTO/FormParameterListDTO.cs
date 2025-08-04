using BBWM.Core.DTO;

namespace BBWM.FormIO.DTO
{
    public class FormParameterListDTO : IDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? Position { get; set; }
        public string TableName { get; set; } = null!;
        public string? KeyField { get; set; }
    }
}