namespace BBF.Reporting.Core.DbModel;

public interface IWidgetEntity
{
    public Guid WidgetSourceId { get; set; }

    public WidgetSource WidgetSource { get; set; }
}