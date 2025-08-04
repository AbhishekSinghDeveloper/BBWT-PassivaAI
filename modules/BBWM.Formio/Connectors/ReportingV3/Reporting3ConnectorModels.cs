namespace BBWM.FormIO.Connectors.ReportingV3;

public class QueryableForm
{
    public string Id { get; set; } = null!;

    public string? ParentFormId { get; set; }

    /// <summary>
    /// E.g. "Employees Survey Form"
    /// </summary>
    public string FormName { get; set; } = null!;

    /// <summary>
    /// E.g. employeesSurvey
    /// </summary>
    public string TableAlias { get; set; } = null!;

    /// <summary>
    /// Forms can have nested related-forms, as form revision grid views.
    /// </summary>
    public IEnumerable<QueryableForm> Children { get; set; } = new List<QueryableForm>();

    /// <summary>
    /// Fields of the form as form view columns.
    /// </summary>
    public IEnumerable<QueryableFormColumn> Columns { get; set; } = new List<QueryableFormColumn>();
}

public class QueryableFormColumn
{
    public string Id { get; set; } = null!;

    /// <summary>
    /// E.g. "User Name"
    /// </summary>
    public string FormName { get; set; } = null!;

    /// <summary>
    /// E.g. "userName"
    /// </summary>
    public string ColumnAlias { get; set; } = null!;

    /// <summary>
    /// Database type of the column.
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Determines if the view column simulates a primary key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Determines if the view column simulates a foreign key.
    /// E.g. "FK_Organizations" referencing Organizations table.
    /// </summary>
    public bool IsForeignKey { get; set; }
}