using BBF.Reporting.Core.Interfaces;

namespace BBF.Reporting.QueryBuilder.Interfaces;

/// <summary>
/// For now, Automatic query builder only remains as a concept in code models in order to highlight
/// that we may have multiple query processing providers. If in future we implement automatic (interactive)
/// query builder, we may reactivate these classes
/// </summary>
public interface IAutoQuerySourceProvider : IQuerySourceProvider, IViewMetadataProvider
{
}