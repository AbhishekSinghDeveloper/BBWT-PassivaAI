using BBWM.Core.DTO;

namespace BBF.Reporting.Core.DTO;

public class WidgetSourcePreloadDTO : IDTO<Guid>
{
    public Guid Id { get; set; }

    public string WidgetType { get; set; } = null!;
}