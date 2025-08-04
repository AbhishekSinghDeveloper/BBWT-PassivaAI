namespace BBF.Reporting.QueryBuilder.Model;

public enum ExclusionMode
{
    ExcludeAll = 0,
    ExcludeOnlyEndTable = 1,
    ExcludeAllExceptEndTable = 2
}

public class TablesRelationExclusion
{
    public string StartTableIdentifier { get; set; } = null!;
    public string? EndTableIdentifier { get; set; }
    public ExclusionMode ExclusionMode { get; set; }
}