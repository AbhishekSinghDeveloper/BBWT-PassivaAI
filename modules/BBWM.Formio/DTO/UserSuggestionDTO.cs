using BBWM.Core.DTO;

namespace BBWM.FormIO.DTO
{
    public class UserSuggestionDTO : IDTO<string>
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string UserName { get; set; } = null!;
    }
}