using BBF.Reporting.Core.Enums;

namespace BBF.Reporting.QueryBuilder.Model;

public class SqlQuerySettings
{
    public string SqlCode { get; set; } = null!;
    public QueryFilterMode? FilterMode { get; set; }
}