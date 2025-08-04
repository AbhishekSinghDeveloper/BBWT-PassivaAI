using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.FormIO.Classes;
using BBWM.FormIO.Interfaces.FormViewInterfaces;
using BBWM.FormIO.Models;
using BBWM.FormIO.Models.FormViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Newtonsoft.Json.Linq;

namespace BBWM.FormIO.Services.FormViewServices;

public class FormViewHelperService : IFormViewHelperService
{
    private delegate string FieldConverter(FormField token, string columnName);

    private readonly IDbContext _context;
    private readonly IFormFieldService _formFieldService;

    private readonly DatabaseType _dbType;
    private readonly ImmutableHashSet<string> _reservedWords;
    private readonly ImmutableDictionary<string, string> _formTypesMapping;
    private readonly ImmutableDictionary<string, FieldConverter> _formTypesConversions;

    public FormViewHelperService(
        IDbContext context,
        IConfiguration configuration,
        IFormFieldService formFieldService)
    {
        _context = context;
        _formFieldService = formFieldService;

        _dbType = configuration.GetDatabaseConnectionSettings().DatabaseType;
        _reservedWords = GetReservedSqlWords();
        _formTypesMapping = GetFormFieldTypesMappings();
        _formTypesConversions = GetFormFieldTypesConversions();
    }

    /// Return a dictionary of mappable form types and its corresponding mappings.
    private ImmutableDictionary<string, string> GetFormFieldTypesMappings()
    {
        return new Dictionary<string, string>
            {
                // Basics types.
                { "textfield", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
                { "textarea", _dbType == DatabaseType.MySql ? "VARCHAR(5000)" : "NVARCHAR(5000)" },
                { "number", "DECIMAL(18, 6)" },
                { "password", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
                { "checkbox", _dbType == DatabaseType.MySql ? "BOOLEAN" : "BIT" },
                { "select", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
                { "radio", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },

                { "reviewerInput", _dbType == DatabaseType.MySql ? "JSON" : "NVARCHAR(5000)" },

                // Advanced types.
                { "email", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
                { "url", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
                { "phoneNumber", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
                { "tags", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
                { "address", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
                { "datetime", _dbType == DatabaseType.MySql ? "DATETIME" : "DATETIME2" },
                { "time", "TIME" },

                { "day", _dbType == DatabaseType.MySql ? "JSON" : "NVARCHAR(5000)" },
                { "currency", _dbType == DatabaseType.MySql ? "JSON" : "NVARCHAR(5000)" },
                { "file_attachments", _dbType == DatabaseType.MySql ? "JSON" : "NVARCHAR(5000)" },
                { "signature", _dbType == DatabaseType.MySql ? "JSON" : "NVARCHAR(5000)" },
                { "bodyMap", _dbType == DatabaseType.MySql ? "JSON" : "NVARCHAR(5000)" },
                { "imageUploader", _dbType == DatabaseType.MySql ? "JSON" : "NVARCHAR(5000)" },

                // Data types.
                { "dataMap", _dbType == DatabaseType.MySql ? "JSON" : "NVARCHAR(5000)" },
                { "editGrid", _dbType == DatabaseType.MySql ? "JSON" : "NVARCHAR(5000)" },

                // Premium types.
                { "custom", _dbType == DatabaseType.MySql ? "JSON" : "NVARCHAR(5000)" }
            }
            .ToImmutableDictionary(StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase));
    }

    /// Return a dictionary of convertible form types and its corresponding conversion functions.
    private static ImmutableDictionary<string, FieldConverter> GetFormFieldTypesConversions()
    {
        return new Dictionary<string, FieldConverter>
            {
                // Basics types.
                { "reviewerInput", ReviewerInputConverter },

                // Advanced types.
                { "day", DayConverter },
                { "currency", CurrencyConverter },
            }
            .ToImmutableDictionary(StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase));

        string ReviewerInputConverter(FormField field, string columnName)
            => $"JSON_UNQUOTE(JSON_EXTRACT({columnName}, '$.value'))";

        string DayConverter(FormField field, string columnName)
            => $"STR_TO_DATE(JSON_UNQUOTE({columnName}), '%m/%d/%Y')";

        string CurrencyConverter(FormField field, string columnName)
        {
            var conversion = $"FORMAT({columnName}, 2)";
            var currency = field.Token.SelectToken("currency")?.Value<string>();

            if (string.IsNullOrEmpty(currency)) return conversion;

            var region = CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Select(culture => new RegionInfo(culture.Name))
                .FirstOrDefault(region => region.ISOCurrencySymbol == currency);

            return region != null ? $"CONCAT(\"{region.CurrencySymbol}\", {conversion})" : conversion;
        }
    }

    // Return a list of reserved sql words that cannot be used as names.
    private static ImmutableHashSet<string> GetReservedSqlWords()
    {
        return new SortedSet<string>
            {
                "select", "from", "where", "join", "insert", "create", "update", "delete", "alter", "drop", "order",
                "into", "values", "and", "or", "not", "between", "like", "as", "count", "sum", "avg", "max", "min"
            }
            .ToImmutableHashSet();
    }

    /// Remove non-alphanumeric nor underscore characters of a string.
    private static string ToAlphanumeric(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var sanitizedValue = Regex.Replace(value.Trim(), @"[^a-zA-Z0-9_.\s]+", "");

        return Regex.Replace(sanitizedValue, @"[.\s]+", "_");
    }

    /// Converts a common string to a valid SQL table/property name.
    /// It is ensured that the returned value does not start with a number and
    /// is not a reserved SQL word (in these cases, an underscore is prefixed).
    private string ToSqlValidName(string? value)
    {
        var sanitizedValue = ToAlphanumeric(value);

        if (string.IsNullOrEmpty(value)) return "_";

        // If the string begins with a number or is a reserved sql word,
        // return underscore plus lowercase version of the string.
        var lowerCaseValue = sanitizedValue.ToLowerInvariant();
        var validName = char.IsNumber(sanitizedValue[0]) || _reservedWords.Contains(lowerCaseValue)
            ? "_" + lowerCaseValue
            : lowerCaseValue;

        return validName;
    }

    // Get a valid column name from a form field.
    private string GetColumnName(FormField field) => ToSqlValidName(field.RelativePath.TrimStart('.'));

    public async Task<string> GetFormUniqueViewName(string? name, CancellationToken ct = default)
    {
        // Clean the name from whitespaces at the beginning and the end of the string.
        // Convert it to valid sql name.
        var sanitizedName = ToSqlValidName(name);

        // If name is larger than 62 characters, take the first 62 characters as candidate form view name.
        if (sanitizedName.Length > 62) sanitizedName = sanitizedName[..62].Trim('_');

        while (sanitizedName.Length > 0)
        {
            // Get all form definitions that has this candidate name as prefix.
            var definitionViewNames = await _context.Set<FormDefinition>()
                .Where(form => form.ViewName != null)
                .Where(form => form.ViewName!.ToLower().StartsWith(sanitizedName))
                .Select(form => form.ViewName)
                .ToListAsync(ct);

            // Get all form revision grids that has this candidate name as prefix.
            var gridViewNames = await _context.Set<FormRevisionGrid>()
                .Where(grid => grid.ViewName.ToLower().StartsWith(sanitizedName))
                .Select(grid => grid.ViewName)
                .ToListAsync(ct);

            // Get all taken names concatenating both lists.
            var names = definitionViewNames.Concat(gridViewNames);

            // From them, filter the ones that has this candidate name as prefix and numeric suffix.
            // Get all the numeric suffixes.
            var suffixes = names
                .Select(formName => formName![sanitizedName.Length..])
                .Where(suffix => Regex.IsMatch(suffix, "^(_[1-9][0-9]*)?$"))
                .Select(suffix => suffix.Length > 1 ? int.Parse(suffix[1..]) : 0)
                .ToList();

            // If there is no suffixes, then this name is not taken and, therefore, valid.
            if (!suffixes.Any()) return sanitizedName.Length < name?.Length ? $"{sanitizedName}_1" : sanitizedName;

            // Sort suffixes by numeric value.
            suffixes.Sort();

            // Initially set the suffix value as the number of times this name has been taken (this suffix is free for sure).
            var suffix = (suffixes
                .Select((suffix, index) => new { Current = suffix, Previous = index > 0 ? suffixes[index - 1] : 0 })
                .FirstOrDefault(item => item.Current - item.Previous > 1)?.Previous + 1 ?? suffixes.Last() + 1).ToString();

            // If the candidate name lenght is 64 or less, then is a valid name.
            if (sanitizedName.Length + suffix.Length + 1 < 65) return $"{sanitizedName}_{suffix}";

            // Otherwise, try again with one character less.
            sanitizedName = sanitizedName[..^1];
        }

        throw new BusinessException("There is no suitable name for the view related to this form: all possible names are taken.");
    }

    // Get the column inside the JSON_TABLE declaration.
    public IEnumerable<FormViewJsonTableColumn> GetJsonTableColumns(FormField root)
    {
        return root.Children
            .Where(field => _formFieldService.IsFormField(field.Type))
            .Select(field =>
            {
                if (!_formTypesMapping.TryGetValue(field.Type, out var columnType)) return null;

                var relativePath = field.RelativePath;
                var fieldName = GetColumnName(field);

                // Declare the [column name] [column type] PATH [path] sentence that creates this column.
                return new FormViewJsonTableColumn { Name = fieldName, Type = columnType, Path = relativePath };
            })
            .Where(column => column is not null)!;
    }

    // Get the final columns of the view.
    public IEnumerable<FormViewColumnItem> GetViewColumns(string viewName, FormField root)
    {
        return root.Children
            .Where(field => _formFieldService.IsFormField(field.Type))
            .Select(field =>
            {
                var fieldName = GetColumnName(field);
                var columnName = $"{viewName}.{fieldName}";

                if (!_formTypesMapping.TryGetValue(field.Type, out var columnType))
                    columnType = _formTypesMapping["textfield"];

                // If this field has declared conversion, obtain conversion expression
                var expression = _formTypesConversions.TryGetValue(field.Type, out var converter)
                    ? converter(field, columnName)
                    : columnName;

                // Declare the column by converting the value directly from the json.
                return new FormViewColumnExpression { Expression = expression, Type = columnType, Alias = fieldName, FormLabel = field.Label };
            });
    }

    public async Task CreateView(string query, CancellationToken ct = default)
    {
        // Execute the query.
        if (_context.Database.GetDbConnection() is MySqlConnection originalConnection)
            try
            {
                var connection = originalConnection.Clone();
                var command = new MySqlCommand(query, connection);
                await connection.OpenAsync(ct);
                await command.ExecuteNonQueryAsync(ct);
                await connection.CloseAsync();
            }
            catch (Exception ex)
            {
                var message = $"Cannot execute query: {query}, an unexpected error occurred. \nException: {ex.Message}";
                throw new BusinessException(message);
            }
    }

    public async Task DeleteView(string viewName, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(viewName)) return;

        var query = $"DROP VIEW IF EXISTS {viewName}";

        // Execute the query.
        if (_context.Database.GetDbConnection() is MySqlConnection originalConnection)
        {
            var connection = originalConnection.Clone();
            var command = new MySqlCommand(query, connection);
            await connection.OpenAsync(ct);
            await command.ExecuteNonQueryAsync(ct);
            await connection.CloseAsync();
        }
    }
}