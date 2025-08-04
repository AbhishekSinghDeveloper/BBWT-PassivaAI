namespace BBF.Reporting.Core.Interfaces;

public interface IQueryProviderFactory
{
    void RegisterQueryProvider<T>(string code);
    IQuerySourceProvider? GetQueryProvider(string code);
    IQuerySourceProvider? GetQueryProvider(Guid querySourceId);
    IEnumerable<IQuerySourceProvider?> GetQueryProviders();

    void RegisterMetadataProvider<T>(string code);
    IViewMetadataProvider? GetMetadataProvider(string code);
    IViewMetadataProvider? GetMetadataProvider(Guid querySourceId);
    IEnumerable<IViewMetadataProvider?> GetMetadataProviders();
}