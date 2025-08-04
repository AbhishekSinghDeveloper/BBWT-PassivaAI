using BBWM.Core.DTO;
using Newtonsoft.Json.Linq;

namespace BBWM.FormIO.DTO.FormVersioningDTOs;

public class FormDataVersioningDTO : IDTO
{
    public int Id { get; set; }
    public JObject? JsonObject { get; set; }
}