using BBWM.Core.DTO;

namespace BBWM.Reporting.DTO;

public class ReportViewDTO : IDTO<Guid>
{
    public Guid Id { get; set; }

    public string UrlSlug { get; set; }

    public string Name { get; set; }

    public bool ShowTitle { get; set; }

    public IEnumerable<SectionDTO> Sections { get; set; } = new List<SectionDTO>();
}
