using System.Collections;
using System.Text.RegularExpressions;
using BBF.Reporting.Core.Enums;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Core.Model;
using BBF.Reporting.Core.Model.Variables;
using BBF.Reporting.TableSet.Interfaces;
using BBF.Reporting.Widget.ControlSet.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.DbDoc.Model;
using SqlKata;
using SqlKata.Execution;

namespace BBF.Reporting.Widget.ControlSet.Services;

public class WidgetControlSetDataService : IWidgetControlSetDataService
{
    private readonly ITableSetService _tableSetService;
    private readonly IContextVariableService _contextVariableService;

    public WidgetControlSetDataService(
        ITableSetService tableSetService,
        IContextVariableService contextVariableService)
    {
        _tableSetService = tableSetService;
        _contextVariableService = contextVariableService;
    }

    public async Task<IEnumerable<dynamic>> GetDropdownData(QueryDataRequest request, CancellationToken ct = default)
    {
        var databaseSource = await _tableSetService.GetQueryDbSource(request.SourceCode, request.FolderId, ct);
        var table = await _tableSetService.GetTable(request.SourceCode, request.FolderId, request.TableId, request.ParentTableId, ct);
        if (table?.Columns == null) throw new BusinessException("Cannot find table metadata: some settings are invalid in control-set item definition.");

        var labelColumn = table.Columns.FirstOrDefault(column => column.Id == request.LabelColumnId);
        var valueColumn = table.Columns.FirstOrDefault(column => column.Id == request.ValueColumnId);
        if (valueColumn is not { ColumnAlias: { } valueColumnAlias } || labelColumn is not { ColumnAlias: { } labelColumnAlias })
            throw new BusinessException("Cannot find table metadata: some settings are invalid in control-set item definition.");

        // Create the query.
        var query = new Query(table.TableAlias);

        // Select both columns, label and value, if they are different.
        // Otherwise, select only on of them to avoid issues related to ambiguous column declaration.
        query = !string.Equals(valueColumnAlias, labelColumnAlias, StringComparison.InvariantCultureIgnoreCase)
            ? query.Select(labelColumnAlias, valueColumnAlias)
            : query.Select(labelColumnAlias);

        // Order the response by label name and restrict the output to 1000 records maximum.
        query.OrderBy(labelColumnAlias).Distinct().Take(1000);

        // If there is no filtering rule, return all the dropdown data.
        if (request.FilterColumnId == null || request.FilterOperator == null)
            return await GetDropdownDataFromQuery(query, databaseSource, labelColumnAlias, valueColumnAlias, ct);

        var filterColumn = table.Columns.FirstOrDefault(column => column.Id == request.FilterColumnId);
        if (filterColumn is not { Name: { } filterColumnName })
            throw new BusinessException("Cannot find table metadata: some settings are invalid in control-set item definition.");

        // If filter operand points to a variable name, check if the variable with the specified name is in the list.
        if (request.FilterOperand != null && request.FilterOperand.StartsWith("#"))
        {
            var variable = request.QueryVariables?.Variables.FirstOrDefault(variable =>
                string.Equals(variable.Name, request.FilterOperand[1..], StringComparison.InvariantCultureIgnoreCase));

            // If the variable is found, filter by variable value.
            query = Where(query, filterColumnName, request.FilterOperator.Value, variable: variable);
        }
        else if (request.FilterOperand != null && request.FilterOperand.StartsWith("@"))
        {
            var contextVariableValue = _contextVariableService.GetVariableValue(request.FilterOperand[1..]);

            // If the variable is found, filter by variable value.
            query = Where(query, filterColumnName, request.FilterOperator.Value, operand: contextVariableValue);
        }
        // Otherwise, filter directly by operand value.
        else query = Where(query, filterColumnName, request.FilterOperator.Value, operand: request.FilterOperand);

        return await GetDropdownDataFromQuery(query, databaseSource, labelColumnAlias, valueColumnAlias, ct);
    }

    private static async Task<IEnumerable<dynamic>> GetDropdownDataFromQuery(Query query, DatabaseSource databaseSource,
        string labelColumnAlias, string valueColumnAlias, CancellationToken ct = default)
    {
        var queryFactory = SqlKataHelper.GetQueryFactory(databaseSource.ConnectionString, databaseSource.DatabaseType);
        var data = await queryFactory.FromQuery(query).GetAsync(cancellationToken: ct);
        var rows = data.Select(row => (row as IDictionary<string, dynamic?>)!);

        return rows.Select(row => new Dictionary<string, dynamic?>
        {
            { "label", row[labelColumnAlias] },
            { "value", row[valueColumnAlias] }
        });
    }

    private static Query Where(Query query, string column, ExpressionOperator expressionOperator, EmittedVariable? variable)
    {
        return variable switch
        {
            null => query,
            { Empty: true, BehaviorOnEmpty: EmittedVariableBehavior.Populate } => query,
            { Empty: true, BehaviorOnEmpty: EmittedVariableBehavior.Clean } => query.WhereRaw("0 = 1"),
            EmittedStringVariable stringVariable => Where(query, column, expressionOperator, value: stringVariable.Value),
            EmittedStringArrayVariable stringArrayVariable => Where(query, column, expressionOperator, value: stringArrayVariable.Value),
            EmittedNumberVariable numberVariable => Where(query, column, expressionOperator, value: numberVariable.Value),
            EmittedNumberArrayVariable numberArrayVariable => Where(query, column, expressionOperator, value: numberArrayVariable.Value),
            EmittedDateVariable dateVariable => Where(query, column, expressionOperator, value: dateVariable.Value),
            EmittedDateArrayVariable dateArrayVariable => Where(query, column, expressionOperator, value: dateArrayVariable.Value),
            EmittedBooleanVariable booleanVariable => Where(query, column, expressionOperator, value: booleanVariable.Value),
            _ => query.WhereRaw("0 = 1")
        };
    }

    private static Query Where(Query query, string column, ExpressionOperator expressionOperator, string? operand)
    {
        return TryParseArrayFromFilterOperand(operand, out var stringArray)
            ? Where(query, column, expressionOperator, value: stringArray)
            : Where(query, column, expressionOperator, value: operand);
    }

    private static Query Where(Query query, string column, ExpressionOperator expressionOperator, object? value)
    {
        // Method to convert object value in a list of string values, if possible.
        List<object?>? GetValueList(object? rawValue) => rawValue is IEnumerable values ? values.Cast<object?>().ToList() : null;

        return expressionOperator switch
        {
            ExpressionOperator.Equals => value == null ? query.WhereNull(column) : query.Where(column, "=", value),
            ExpressionOperator.NotEquals => value == null ? query.WhereNotNull(column) : query.Where(column, "!=", value),
            ExpressionOperator.MoreOrEqual => value == null ? query.WhereNull(column) : query.Where(column, ">=", value),
            ExpressionOperator.LessOrEqual => value == null ? query.WhereNotNull(column) : query.Where(column, "<=", value),

            ExpressionOperator.More => query.Where(column, ">", value),
            ExpressionOperator.Less => query.Where(column, "<", value),

            ExpressionOperator.StartsWith when value is string sentence => query.WhereLike(column, $"{sentence}%"),
            ExpressionOperator.EndsWith when value is string sentence => query.WhereLike(column, $"%{sentence}"),

            ExpressionOperator.In when GetValueList(value) is { Count: > 0 } values
                => values.Any(item => item == null)
                    ? query.WhereNull(column).OrWhereIn(column, values.Where(item => item != null))
                    : query.WhereIn(column, values),

            ExpressionOperator.NotIn when GetValueList(value) is { Count: > 0 } values
                => values.Any(item => item == null)
                    ? query.WhereNotNull(column).WhereNotIn(column, values.Where(item => item != null))
                    : query.WhereNotIn(column, values),

            ExpressionOperator.Between when GetValueList(value) is { Count: > 1 } values
                => query.WhereBetween(column, values[0], values[1]),

            _ => query.WhereRaw("0 = 1")
        };
    }

    private static bool TryParseArrayFromFilterOperand(string? value, out string[] array)
    {
        array = Array.Empty<string>();

        const string pattern = @"^\s*[\(\[]\s*([^\(\)\[\],]+(?:\s*,\s*[^\(\)\[\],]+)*)?\s*[\]\)]\s*$";
        if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, pattern)) return false;

        array = value.Trim()[1..^1].Split(',').ToArray();
        return true;
    }
}