using BBF.Reporting.Core.Enums;
using BBF.Reporting.Core.ModelBinders;

namespace BBF.Reporting.Core.Model;

public class QueryDataRequest
{
    public string TableId { get; set; } = null!;
    public string FolderId { get; set; } = null!;
    public string SourceCode { get; set; } = null!;
    public string ValueColumnId { get; set; } = null!;
    public string LabelColumnId { get; set; } = null!;
    public string? ParentTableId { get; set; }
    public string? FilterColumnId { get; set; }
    public ExpressionOperator? FilterOperator { get; set; }
    [UrlDecoded] public string? FilterOperand { get; set; }
    public QueryVariables? QueryVariables { get; set; }
}