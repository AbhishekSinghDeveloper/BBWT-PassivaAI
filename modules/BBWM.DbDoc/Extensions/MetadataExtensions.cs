using BBWM.Core.Extensions;
using BBWM.DbDoc.DbSchemas.SchemaModels;
using BBWM.DbDoc.Model;
using System.Text.RegularExpressions;

namespace BBWM.DbDoc.Extensions;

public static partial class MetadataExtensions
{
    public static ColumnMetadata FromSchemaColumn(this ColumnMetadata columnMetadata, DbSchemaColumn schemaColumn)
    {
        columnMetadata.Title = ParseDbColumnTitlePresentation(schemaColumn.ColumnName);
        return columnMetadata;
    }

    [GeneratedRegex("(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+|[0-9*]+|[a-z]+)")]
    private static partial Regex RgxWordsList();

    private static string ParseDbColumnTitlePresentation(string columnName)
    {
        var words = RgxWordsList().Matches(columnName).Select(m => m.Value);

        // Consider some abbreviations
        var abbrList = new string[] { "id", "ip", "http", "https", "xml", "xls" };
        words = words.Select(o => abbrList.Contains(o.ToLowerInvariant()) ? o.ToUpperInvariant() : o);

        return string.Join(" ", words).ToTitlePhrase();
    }
}
