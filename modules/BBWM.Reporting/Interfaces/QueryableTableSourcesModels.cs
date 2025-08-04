using BBWM.DbDoc.DbSchemas.SchemaModels;

namespace BBWM.Reporting.Interfaces;

// TODO: need to find proper location for files of these classes

public class QueryableTableSource
{
    /// <summary>
    /// A code assosiated with a source, providing table schemas for reporting queries.
    /// It's stored for each query table's record.
    /// </summary>
    public string SourceCode { get; set; }

    public string SourceName { get; set; }

    public IEnumerable<QueryableTable> Tables { get; set; }
}

public class QueryableTable
{
    //TODO: likely we'll add additional fields to provide details of table ownership:
    // user's personal / organization / for all

    /// <summary>
    /// E.g. "Employees Survey Form"
    /// </summary>
    public string FriendlyName { get; set; }

    /// <summary>
    /// E.g. employeesSurvey
    /// </summary>
    public string SourceTableAlias { get; set; }

    /// <summary>
    /// E.g. with TableName =  org102_view_employees_survey
    /// </summary>
    public DbSchemaTable SchemaTable { get; set; }

    public IEnumerable<DbSchemaColumn> SchemaColumns { get; set; }
}