using BBWM.Core.DTO;

namespace BBWM.StaticPages;

public class StaticPageDTO : IDTO
{
    public int Id { get; set; }

    public string Alias { get; set; }

    public string Heading { get; set; }

    public string Contents { get; set; }

    public string ContentPreview { get; set; }

    public DateTime LastUpdated { get; set; }
}
