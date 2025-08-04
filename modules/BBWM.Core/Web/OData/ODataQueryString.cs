using System.Text.RegularExpressions;

namespace BBWM.Core.Web.OData;

/// <summary>
/// Represents additional functional to work with OData query strings.
/// </summary>
public class ODataQueryString
{
    /// <summary>
    /// Creates new instance based on string value.
    /// </summary>
    /// <param name="value">OData query string.</param>
    public ODataQueryString(string value) => Value = value;


    /// <summary>
    /// Gets string value of the OData query.
    /// </summary>
    public string Value { get; private set; }

    /// <summary>
    /// Determines if the filter exists.
    /// </summary>
    /// <param name="filterName">Filter name</param>
    public bool ContainsFilter(string filterName) =>
        GetFullStatement("filter")?.Contains(filterName, StringComparison.InvariantCultureIgnoreCase) ?? false;

    /// <summary>
    /// Determines if the expanded property exists.
    /// </summary>
    /// <param name="propertyName">Name of expanded property.</param>
    public bool ContainsExpand(string propertyName) =>
        GetFullStatement("expand")?.Contains(propertyName, StringComparison.InvariantCultureIgnoreCase) ?? false;

    /// <summary>
    /// Returns the name of the property to sort by.
    /// </summary>
    /// <returns>String property name value or "null" if this statement not specified.</returns>
    public string GetOrderFieldName() => GetStatement("orderby")?.Split(" ")[0];

    /// <summary>
    /// Try to get full string value of specified statement.
    /// </summary>
    /// <param name="statementName">Statement name.</param>
    /// <returns>Statement value if exists or null otherwise.</returns>
    public string GetStatement(string statementName) => GetFullStatement(statementName);

    /// <summary>
    /// Extract all filter values.
    /// </summary>
    /// <typeparam name="T">Type that all values should be cast to.</typeparam>
    /// <param name="filterName">Filters name.</param>
    /// <returns>Array of all values for all filters with specified name.</returns>
    public T[] GetFilterValues<T>(string filterName) =>
        GetFilterStatementsByName(filterName).SelectMany(GetFilterArguments<T>).ToArray();

    /// <summary>
    /// Removes all filters with specified name.
    /// </summary>
    /// <param name="filterName">Filters name.</param>
    public void RemoveFilter(string filterName)
    {
        var removingFilterStatements = GetFilterStatementsByName(filterName);

        if (removingFilterStatements is null) return;

        var filtersOldStatement = GetFullStatement("filter");
        var filtersNewStatement = filtersOldStatement;
        foreach (var removingStatement in removingFilterStatements)
        {
            filtersNewStatement = filtersNewStatement.Replace($"{removingStatement} and", "");
            filtersNewStatement = filtersNewStatement.Replace($"and {removingStatement}", "");
            filtersNewStatement = filtersNewStatement.Replace($"{removingStatement} or", "");
            filtersNewStatement = filtersNewStatement.Replace($"or {removingStatement}", "");
            filtersNewStatement = filtersNewStatement.Replace($"{removingStatement}", "");
        }

        if (string.IsNullOrWhiteSpace(filtersNewStatement))
        {
            Value = Value.Replace($"&$filter={filtersOldStatement}", "");
            Value = Value.Replace($"$filter={filtersOldStatement}&", "");
            Value = Value.Replace($"$filter={filtersOldStatement}", "");
        }
        else
        {
            Value = Value.Replace(filtersOldStatement, filtersNewStatement);
        }
    }

    public void RemoveOrdering()
    {
        var orderFullStatement = GetFullStatement("orderby");

        if (!string.IsNullOrWhiteSpace(orderFullStatement))
        {
            Value = Value.Replace($"&$orderby={orderFullStatement}", "");
            Value = Value.Replace($"$orderby={orderFullStatement}&", "");
            Value = Value.Replace($"$orderby={orderFullStatement}", "");
        }
    }


    private string GetFullStatement(string statementName)
    {
        var match = new Regex($"\\${statementName}=(.*?)(&|$)").Match(Value);
        if (match.Success)
            return match.Groups[1].Value;

        return null;
    }

    private string[] GetFilterStatementsByName(string name)
    {
        var fullStatement = GetFullStatement("filter")?
            .Replace(" OR ", " or ", StringComparison.InvariantCultureIgnoreCase)
            .Replace(" AND ", " and ", StringComparison.InvariantCultureIgnoreCase);

        return fullStatement?
            .Split(" or ")
            .SelectMany(x => x.Split(" and "))
            .Where(x => x.Contains(name, StringComparison.InvariantCultureIgnoreCase))
            .ToArray();
    }

    private static T[] GetFilterArguments<T>(string filterStatement) =>
        new[]
            {
                    new Regex(",\\s?[\"\\']?([\\w\\.]*)[\"\\']?"),
                    new Regex("\\s+[\"\\']?([\\w\\.]*)[\"\\']?$"),
            }.Aggregate(new List<T>(), (accumulator, regex) =>
            {
                accumulator.AddRange(regex.Matches(filterStatement)
                    .Select(x => (T)Convert.ChangeType(x.Groups[1].Value, typeof(T))));
                return accumulator;
            })
            .ToArray();
}
