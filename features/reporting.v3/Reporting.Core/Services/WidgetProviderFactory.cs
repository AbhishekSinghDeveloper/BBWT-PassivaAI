using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;

namespace BBF.Reporting.Core.Services;

public class WidgetProviderFactory : IWidgetProviderFactory
{
    private readonly IDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    public WidgetProviderFactory(IServiceProvider serviceProvider, IDbContext context)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    private static readonly Dictionary<string, Type> WidgetSourceProviderTypesDic = new();

    public void RegisterWidgetProvider<T>(string code)
    {
        var pType = typeof(T);
        if (_serviceProvider.GetService(pType) is null)
            throw new ObjectNotExistsException($"Provider's interface '{pType.Name}' not found");
        WidgetSourceProviderTypesDic.Add(code, pType);
    }

    public IWidgetSourceProvider? GetWidgetProvider(string code)
        => WidgetSourceProviderTypesDic.TryGetValue(code, out var serviceType)
            ? _serviceProvider.GetService(serviceType) as IWidgetSourceProvider
            : null;

    public IWidgetSourceProvider? GetWidgetProvider(Guid widgetSourceId)
    {
        var widgetSourceType = _context.Set<WidgetSource>()
            .Where(source => source.Id == widgetSourceId)
            .Select(source => source.WidgetType)
            .First();

        return GetWidgetProvider(widgetSourceType);
    }

    public IEnumerable<IWidgetSourceProvider?> GetWidgetProviders()
        => WidgetSourceProviderTypesDic.Keys.Select(GetWidgetProvider);
}