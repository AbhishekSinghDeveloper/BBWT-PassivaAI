namespace BBF.Reporting.Core.DbModel;

public interface IQueryEntity
{
    public Guid QuerySourceId { get; set; }

    public QuerySource QuerySource { get; set; }
}