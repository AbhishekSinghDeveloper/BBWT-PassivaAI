using System.Collections;

namespace BBWM.DataProcessing.Classes;

/// <summary>
/// Defines the set of column definitions for data import
/// </summary>
public class ColumnsDefinitionsCollection : IEnumerable<ColumnDefinition>
{
    /// <summary>
    /// Entire collection
    /// </summary>
    private readonly List<ColumnDefinition> columnDefinitions = new List<ColumnDefinition>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public ColumnsDefinitionsCollection()
    {
    }

    public int Count { get => columnDefinitions.Count; }

    /// <summary>
    /// Custom constructor
    /// </summary>
    /// <param name="columns">Columns definitions array</param>
    public ColumnsDefinitionsCollection(params ColumnDefinition[] columns)
    {
        foreach (var column in columns)
            AddColumnInternal(column);
    }


    /// <summary>
    /// Adds column definition to set
    /// </summary>
    /// <param name="columnDef">Column definition</param>
    /// <returns>this</returns>
    public ColumnsDefinitionsCollection AddColumn(ColumnDefinition columnDef)
    {
        AddColumnInternal(columnDef);
        return this;
    }

    /// <summary>
    /// Performs basic validation and adds column definition to set
    /// </summary>
    /// <param name="columnDef">Column definition</param>
    private void AddColumnInternal(ColumnDefinition columnDef)
    {
        if (columnDef is null)
            throw new ArgumentNullException("columnDef");
        if (columnDef.OrderNumber < 1)
            throw new ArgumentException("Column number must be greater or equal than 1");
        if (columnDefinitions.Any(col => col.OrderNumber == columnDef.OrderNumber))
            throw new ArgumentException("Column with the specified number already exists");

        columnDefinitions.Add(columnDef);
        columnDefinitions.Sort((lhs, rhs) => lhs.OrderNumber - rhs.OrderNumber);
    }

    public IEnumerator<ColumnDefinition> GetEnumerator() => columnDefinitions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => columnDefinitions.GetEnumerator();
}
