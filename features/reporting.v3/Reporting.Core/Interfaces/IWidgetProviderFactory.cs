namespace BBF.Reporting.Core.Interfaces;

public interface IWidgetProviderFactory
{
    void RegisterWidgetProvider<T>(string code);

    IWidgetSourceProvider? GetWidgetProvider(string code);

    IWidgetSourceProvider? GetWidgetProvider(Guid widgetSourceId);

    IEnumerable<IWidgetSourceProvider?> GetWidgetProviders();
}