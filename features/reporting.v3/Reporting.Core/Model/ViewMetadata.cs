namespace BBF.Reporting.Core.Model;

public class ViewMetadata
{
    public IEnumerable<ViewMetadataColumn> Columns { get; set; } = Enumerable.Empty<ViewMetadataColumn>();

    public IEnumerable<CustomColumnType> CustomColumnTypes { get; set; } = Enumerable.Empty<CustomColumnType>();
}
